using HidroRec.Backend.Application.DTOs.DataFusion;
using HidroRec.Backend.Application.Interfaces;
using HidroRec.Backend.Domain.Entities;
using HidroRec.Backend.Domain.Enums;
using HidroRec.Backend.Infrastructure.Data;
using HidroRec.Backend.Infrastructure.ExternalServices;
using Microsoft.EntityFrameworkCore;

namespace HidroRec.Backend.Application.Services;

public sealed class DataFusionService(
    HidroRecDbContext context,
    IWeatherService weatherService,
    ITideService tideService,
    IExternalDataCache externalDataCache,
    ILogger<DataFusionService> logger) : IDataFusionService
{
    private static readonly (decimal Latitude, decimal Longitude) RecifeCenter = (-8.0476m, -34.8770m);

    public async Task<OperationalDataFusionSnapshotDto> GetSnapshotAsync(decimal? latitude, decimal? longitude, string? bairro, CancellationToken cancellationToken)
    {
        var traceId = Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();
        var location = await ResolveLocationAsync(latitude, longitude, bairro, cancellationToken);
        var observed = await weatherService.GetObservedWeatherAsync(cancellationToken);
        var forecast = await weatherService.GetForecastWeatherAsync(cancellationToken);
        var tide = await tideService.GetTideReadingAsync(cancellationToken);
        var history = await BuildHistoricalContextAsync(location, cancellationToken);
        var sources = BuildSources(observed, forecast, tide);
        var risk = BuildRisk(observed, forecast, tide, history, sources);
        var warnings = BuildWarnings(location, sources, risk, history);
        var dataQualityScore = CalculateDataQualityScore(sources, warnings);

        var snapshot = new OperationalDataFusionSnapshotDto
        {
            TraceId = traceId,
            GeneratedAtUtc = DateTime.UtcNow,
            DataQualityScore = dataQualityScore,
            Location = location,
            Climate = new ClimateFusionDto
            {
                RainObserved24hMm = observed.Rain24hMm,
                CurrentRainMm = observed.CurrentRainMm,
                Forecast6hMm = forecast.Forecast6hMm,
                Forecast12hMm = forecast.Forecast12hMm,
                Forecast24hMm = forecast.Forecast24hMm,
                PeakHourlyRainMm = forecast.PeakHourlyMm,
                PeakHourlyAtUtc = forecast.PeakHourlyAtUtc,
                MaxProbabilityPercent = forecast.MaxProbabilityPercent,
                TemperatureCelsius = observed.TemperatureCelsius,
                Reliability = decimal.Round(sources.Where(source => source.Category != "Mare").Average(source => source.Reliability), 2),
                UpdatedAtUtc = Max(observed.ObservedAtUtc, observed.CurrentAtUtc, forecast.GeneratedAtUtc),
                Summary = BuildClimateSummary(observed, forecast)
            },
            Tide = new TideFusionDto
            {
                CurrentLevelMeters = tide.CurrentLevelMeters,
                NextExtremeLevelMeters = tide.NextExtremeLevelMeters,
                NextExtremeAtUtc = tide.NextExtremeAtUtc,
                Max24hLevelMeters = tide.Max24hLevelMeters,
                Trend = tide.Trend,
                HarborName = tide.HarborName,
                Reliability = tide.Reliability,
                UpdatedAtUtc = tide.UpdatedAtUtc,
                Summary = tide.Summary
            },
            History = history,
            Risk = risk,
            Sources = sources,
            Warnings = warnings
        };

        await TrackSnapshotAsync(snapshot, cancellationToken);
        return snapshot;
    }

    private async Task<LocationContextDto> ResolveLocationAsync(decimal? latitude, decimal? longitude, string? bairro, CancellationToken cancellationToken)
    {
        var areas = await context.AreasMonitoradas
            .AsNoTracking()
            .Include(area => area.Bairro)
            .Include(area => area.Regiao)
            .Where(area => area.Ativa && !area.Excluido)
            .ToListAsync(cancellationToken);

        var effectiveLatitude = latitude ?? RecifeCenter.Latitude;
        var effectiveLongitude = longitude ?? RecifeCenter.Longitude;

        var resolutionMethod = "city-default";
        var candidates = areas.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(bairro))
        {
            candidates = candidates.Where(area =>
                string.Equals(area.Bairro?.Nome, bairro, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(area.Cobertura, bairro, StringComparison.OrdinalIgnoreCase) ||
                area.Nome.Contains(bairro, StringComparison.OrdinalIgnoreCase));
            resolutionMethod = "bairro-match";
        }

        var selectedArea = candidates
            .OrderBy(area => CalculateDistanceKm(effectiveLatitude, effectiveLongitude, area.Latitude, area.Longitude))
            .ThenByDescending(area => area.CriticidadeOperacional)
            .FirstOrDefault();

        if (selectedArea is null)
        {
            resolutionMethod = latitude.HasValue && longitude.HasValue ? "nearest-coordinates" : "city-default";
            selectedArea = areas
                .OrderBy(area => CalculateDistanceKm(effectiveLatitude, effectiveLongitude, area.Latitude, area.Longitude))
                .ThenByDescending(area => area.CriticidadeOperacional)
                .FirstOrDefault();
        }

        if (selectedArea is not null && latitude.HasValue && longitude.HasValue && resolutionMethod == "bairro-match")
        {
            resolutionMethod = "bairro-and-coordinates";
        }

        var distanceKm = selectedArea is null
            ? (decimal?)null
            : decimal.Round(CalculateDistanceKm(effectiveLatitude, effectiveLongitude, selectedArea.Latitude, selectedArea.Longitude), 2);

        return new LocationContextDto
        {
            AreaMonitoradaId = selectedArea?.Id,
            Latitude = latitude ?? selectedArea?.Latitude ?? RecifeCenter.Latitude,
            Longitude = longitude ?? selectedArea?.Longitude ?? RecifeCenter.Longitude,
            Bairro = selectedArea?.Bairro?.Nome ?? bairro ?? "Recife",
            Regiao = selectedArea?.Regiao?.Nome ?? "Recife",
            AreaMonitorada = selectedArea?.Nome ?? "Recife",
            ResolutionMethod = resolutionMethod,
            DistanceKm = distanceKm,
            CoverageContext = selectedArea is null
                ? "Cobertura municipal padrao"
                : $"{selectedArea.Nome} | criticidade operacional {selectedArea.CriticidadeOperacional}"
        };
    }

    private async Task<HistoricalContextDto> BuildHistoricalContextAsync(LocationContextDto location, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        IQueryable<Reporte> query = context.Reportes
            .AsNoTracking()
            .Where(item => !item.Excluido);

        if (location.AreaMonitoradaId.HasValue)
        {
            query = query.Where(item => item.AreaMonitoradaId == location.AreaMonitoradaId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(location.Bairro))
        {
            query = query.Where(item =>
                item.BairroNome == location.Bairro ||
                (item.Bairro != null && item.Bairro.Nome == location.Bairro));
        }

        var reportes24h = await query.CountAsync(item => item.DataOcorrencia >= now.AddHours(-24), cancellationToken);
        var alagamentos24h = await query.CountAsync(item =>
            item.DataOcorrencia >= now.AddHours(-24) &&
            item.Status != StatusReporte.Arquivado &&
            item.Status != StatusReporte.Resolvido &&
            (item.Severidade == SeveridadeReporte.Alagamento || item.Severidade == SeveridadeReporte.AlagamentoCritico), cancellationToken);
        var confirmados24h = await query.CountAsync(item =>
            item.DataOcorrencia >= now.AddHours(-24) &&
            item.Status == StatusReporte.Confirmado, cancellationToken);
        var reportes7d = await query.CountAsync(item => item.DataOcorrencia >= now.AddDays(-7), cancellationToken);
        var recorrencia30d = await query.CountAsync(item => item.DataOcorrencia >= now.AddDays(-30), cancellationToken);
        var lastOccurrenceAtUtc = await query
            .OrderByDescending(item => item.DataOcorrencia)
            .Select(item => (DateTime?)item.DataOcorrencia)
            .FirstOrDefaultAsync(cancellationToken);

        return new HistoricalContextDto
        {
            Reportes24h = reportes24h,
            Alagamentos24h = alagamentos24h,
            Confirmados24h = confirmados24h,
            Reportes7d = reportes7d,
            Recorrencia30d = recorrencia30d,
            LastOccurrenceAtUtc = lastOccurrenceAtUtc,
            HistoricalPressure = decimal.Round(Math.Min(100m, (reportes24h * 6.5m) + (alagamentos24h * 12m) + (reportes7d * 1.8m)), 2),
            Summary = $"{reportes24h} reportes em 24h | {alagamentos24h} alagamentos ativos em 24h | {confirmados24h} confirmados em 24h | {recorrencia30d} ocorrencias em 30 dias."
        };
    }

    private static IReadOnlyCollection<DataSourceStatusDto> BuildSources(
        WeatherObservedReading observed,
        WeatherForecastReading forecast,
        TideReading tide)
    {
        return
        [
            new DataSourceStatusDto
            {
                TraceCode = "weather-observed",
                Source = "APAC + Open-Meteo",
                Category = "Clima observado",
                Status = ResolveSourceStatus(observed.IsFresh, observed.Summary),
                TimestampUtc = Max(observed.ObservedAtUtc, observed.CurrentAtUtc),
                FreshnessMinutes = ResolveFreshnessMinutes(Max(observed.ObservedAtUtc, observed.CurrentAtUtc)),
                Reliability = observed.Reliability,
                IsFresh = observed.IsFresh,
                FallbackUsed = observed.Summary.Contains("cache", StringComparison.OrdinalIgnoreCase),
                FreshnessLabel = ResolveFreshnessLabel(ResolveFreshnessMinutes(Max(observed.ObservedAtUtc, observed.CurrentAtUtc)), observed.IsFresh),
                Summary = observed.Summary
            },
            new DataSourceStatusDto
            {
                TraceCode = "weather-forecast",
                Source = "Open-Meteo",
                Category = "Previsao horaria",
                Status = ResolveSourceStatus(forecast.IsFresh, forecast.Summary),
                TimestampUtc = forecast.GeneratedAtUtc,
                FreshnessMinutes = ResolveFreshnessMinutes(forecast.GeneratedAtUtc),
                Reliability = forecast.Reliability,
                IsFresh = forecast.IsFresh,
                FallbackUsed = forecast.Summary.Contains("cache", StringComparison.OrdinalIgnoreCase),
                FreshnessLabel = ResolveFreshnessLabel(ResolveFreshnessMinutes(forecast.GeneratedAtUtc), forecast.IsFresh),
                Summary = forecast.Summary
            },
            new DataSourceStatusDto
            {
                TraceCode = "tide-observed",
                Source = "Tabua de Mares / DHN",
                Category = "Mare",
                Status = ResolveSourceStatus(tide.IsFresh, tide.Summary),
                TimestampUtc = tide.UpdatedAtUtc,
                FreshnessMinutes = ResolveFreshnessMinutes(tide.UpdatedAtUtc),
                Reliability = tide.Reliability,
                IsFresh = tide.IsFresh,
                FallbackUsed = tide.Summary.Contains("cache", StringComparison.OrdinalIgnoreCase),
                FreshnessLabel = ResolveFreshnessLabel(ResolveFreshnessMinutes(tide.UpdatedAtUtc), tide.IsFresh),
                Summary = tide.Summary
            }
        ];
    }

    private static RiskAssessmentDto BuildRisk(
        WeatherObservedReading observed,
        WeatherForecastReading forecast,
        TideReading tide,
        HistoricalContextDto history,
        IReadOnlyCollection<DataSourceStatusDto> sources)
    {
        var observedFreshnessFactor = observed.IsFresh ? 1m : 0.35m;
        var forecastFreshnessFactor = forecast.IsFresh ? 1m : 0.40m;
        var tideFreshnessFactor = tide.IsFresh ? 1m : 0.45m;

        var observedRainScore = ScoreLinear(observed.Rain24hMm, 10m, 50m, 30m) * observedFreshnessFactor;
        var currentRainScore = ScoreLinear(observed.CurrentRainMm, 0.2m, 3m, 6m) * observedFreshnessFactor;
        var forecast6hScore = ScoreLinear(forecast.Forecast6hMm, 1m, 8m, 16m) * forecastFreshnessFactor;
        var forecast24hScore = ScoreLinear(forecast.Forecast24hMm, 4m, 20m, 12m) * forecastFreshnessFactor;
        var peakHourlyScore = ScoreLinear(forecast.PeakHourlyMm, 0.5m, 3m, 10m) * forecastFreshnessFactor;
        var tideCurrentScore = ScoreLinear(tide.CurrentLevelMeters, 1.5m, 2.8m, 10m) * tideFreshnessFactor;
        var tideMaxScore = ScoreLinear(tide.Max24hLevelMeters, 1.8m, 3.0m, 10m) * tideFreshnessFactor;
        var floodHistoryScore = ScoreLinear(history.Alagamentos24h, 1m, 4m, 8m);
        var reports24hScore = ScoreLinear(history.Reportes24h, 1m, 6m, 6m);
        var reports7dScore = ScoreLinear(history.Reportes7d, 2m, 15m, 3m);

        var rawScore = observedRainScore +
                       currentRainScore +
                       forecast6hScore +
                       forecast24hScore +
                       peakHourlyScore +
                       tideCurrentScore +
                       tideMaxScore +
                       floodHistoryScore +
                       reports24hScore +
                       reports7dScore;

        var reliability = decimal.Round(sources.Average(source => source.Reliability), 2);
        var staleSources = sources.Count(source => !source.IsFresh);
        var reliabilityFactor = 0.70m + (reliability * 0.30m);
        var freshnessPenalty = Math.Max(0.72m, 1m - (staleSources * 0.08m));
        var adjustedScore = (int)Math.Round(Math.Min(100m, rawScore * reliabilityFactor * freshnessPenalty));
        var level = ResolveRiskLevel(adjustedScore);
        var rainContribution = decimal.Round(observedRainScore + currentRainScore + forecast6hScore + forecast24hScore + peakHourlyScore, 2);
        var tideContribution = decimal.Round(tideCurrentScore + tideMaxScore, 2);
        var historyContribution = decimal.Round(floodHistoryScore + reports24hScore + reports7dScore, 2);

        var drivers = new List<string>();
        if (observed.Rain24hMm > 0m)
        {
            drivers.Add($"{observed.Rain24hMm:0.0} mm observados em 24h");
        }

        if (forecast.Forecast24hMm > 0m)
        {
            drivers.Add($"{forecast.Forecast24hMm:0.0} mm previstos nas proximas 24h");
        }

        if (forecast.PeakHourlyMm > 0m)
        {
            drivers.Add($"pico de {forecast.PeakHourlyMm:0.0} mm/h por volta de {forecast.PeakHourlyAtUtc.ToLocalTime():HH\\h}");
        }

        drivers.Add($"mare em {tide.CurrentLevelMeters:0.00} m");
        drivers.Add($"{history.Alagamentos24h} alagamentos ativos nas ultimas 24h");

        return new RiskAssessmentDto
        {
            Score = adjustedScore,
            Level = level,
            Reliability = reliability,
            RainContribution = rainContribution,
            TideContribution = tideContribution,
            HistoryContribution = historyContribution,
            DecisionPriority = ResolveDecisionPriority(adjustedScore),
            RecommendedAction = ResolveRecommendedAction(adjustedScore, forecast.Forecast6hMm, forecast.Forecast24hMm, tide.CurrentLevelMeters, history.Alagamentos24h),
            CriticalWindow = ResolveCriticalWindow(forecast.Forecast6hMm, forecast.Forecast12hMm, forecast.Forecast24hMm, forecast.PeakHourlyAtUtc),
            Drivers = drivers,
            Summary = $"{level} | score {adjustedScore}/100 | leitura fusionada de chuva, mare e historico."
        };
    }

    private async Task TrackSnapshotAsync(OperationalDataFusionSnapshotDto snapshot, CancellationToken cancellationToken)
    {
        var cacheKey = $"data-fusion:trace:{snapshot.Location.AreaMonitoradaId?.ToString() ?? snapshot.Location.Bairro.ToLowerInvariant()}";
        var signature = string.Join('|',
            snapshot.Risk.Level,
            snapshot.Risk.Score / 10,
            snapshot.Location.ResolutionMethod,
            string.Join(',', snapshot.Sources.Select(source => $"{source.TraceCode}:{source.Status}:{source.FallbackUsed}")),
            snapshot.Warnings.Count);

        if (externalDataCache.TryGet<DataFusionTraceState>(cacheKey, out var previous) &&
            previous is not null &&
            previous.Signature == signature &&
            DateTime.UtcNow.Subtract(previous.LoggedAtUtc).TotalMinutes < 12)
        {
            return;
        }

        var level = snapshot.Warnings.Count > 0 || snapshot.Risk.Score >= 50
            ? snapshot.Risk.Score >= 75 ? "Critical" : "Warning"
            : "Information";

        var eventName = snapshot.Warnings.Count > 0
            ? "data-fusion-alert"
            : snapshot.Risk.Score >= 50
                ? "data-fusion-risk"
                : "data-fusion-snapshot";

        var message = $"{snapshot.Location.AreaMonitorada} | risco {snapshot.Risk.Score}/100 ({snapshot.Risk.Level}) | chuva 24h {snapshot.Climate.RainObserved24hMm:0.0} mm | prox24h {snapshot.Climate.Forecast24hMm:0.0} mm | mare {snapshot.Tide.CurrentLevelMeters:0.0} m";
        var sourceSummary = string.Join(',', snapshot.Sources.Select(source => $"{source.TraceCode}:{source.Status}"));
        var warningSummary = snapshot.Warnings.Count == 0
            ? "none"
            : string.Join(',', snapshot.Warnings.Take(3).Select(CompactToken));
        var traceContext = $"data-fusion|trace={snapshot.TraceId}|area={CompactToken(snapshot.Location.AreaMonitorada)}|method={snapshot.Location.ResolutionMethod}|quality={snapshot.DataQualityScore:0}|risk={snapshot.Risk.Score}|sources={sourceSummary}|warnings={warningSummary}";

        try
        {
            context.LogsSistema.Add(new LogSistema
            {
                Nivel = level,
                Evento = eventName,
                Mensagem = message,
                Contexto = traceContext.Length > 440 ? traceContext[..440] : traceContext
            });

            await context.SaveChangesAsync(cancellationToken);
            externalDataCache.Set(cacheKey, new DataFusionTraceState
            {
                Signature = signature,
                LoggedAtUtc = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao registrar rastreamento de data fusion.");
        }
    }

    private static IReadOnlyCollection<string> BuildWarnings(
        LocationContextDto location,
        IReadOnlyCollection<DataSourceStatusDto> sources,
        RiskAssessmentDto risk,
        HistoricalContextDto history)
    {
        var warnings = new List<string>();

        foreach (var source in sources.Where(source => !source.IsFresh || source.FallbackUsed))
        {
            warnings.Add($"{source.Source} em {source.Status.ToLowerInvariant()}");
        }

        if (location.DistanceKm.HasValue && location.DistanceKm.Value > 2.5m)
        {
            warnings.Add($"Area resolvida a {location.DistanceKm.Value:0.0} km do ponto consultado");
        }

        if (location.ResolutionMethod is "nearest-coordinates" or "city-default")
        {
            warnings.Add($"Leitura territorial resolvida por {location.ResolutionMethod}");
        }

        if (history.LastOccurrenceAtUtc.HasValue && DateTime.UtcNow.Subtract(history.LastOccurrenceAtUtc.Value).TotalDays > 14)
        {
            warnings.Add("Historico territorial recente limitado");
        }

        if (risk.Reliability < 0.75m)
        {
            warnings.Add("Confiabilidade fusionada abaixo do ideal");
        }

        return warnings.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static decimal CalculateDataQualityScore(IReadOnlyCollection<DataSourceStatusDto> sources, IReadOnlyCollection<string> warnings)
    {
        var baseScore = sources.Count == 0 ? 0m : sources.Average(source => source.Reliability) * 100m;
        var freshnessPenalty = sources.Count(source => !source.IsFresh) * 5m;
        var fallbackPenalty = sources.Count(source => source.FallbackUsed) * 7m;
        var warningPenalty = warnings.Count * 2m;
        return decimal.Round(Math.Max(0m, Math.Min(100m, baseScore - freshnessPenalty - fallbackPenalty - warningPenalty)), 2);
    }

    private static string BuildClimateSummary(WeatherObservedReading observed, WeatherForecastReading forecast)
    {
        return $"{observed.Rain24hMm:0.0} mm observados em 24h | chuva agora {observed.CurrentRainMm:0.0} mm | prox. 6h {forecast.Forecast6hMm:0.0} mm | prox. 24h {forecast.Forecast24hMm:0.0} mm.";
    }

    private static string ResolveSourceStatus(bool isFresh, string summary)
    {
        if (summary.Contains("cache", StringComparison.OrdinalIgnoreCase))
        {
            return "Fallback";
        }

        return isFresh ? "Ativo" : "Desatualizado";
    }

    private static string ResolveRiskLevel(int score)
    {
        if (score >= 75) return "Critico";
        if (score >= 50) return "Alto";
        if (score >= 25) return "Medio";
        return "Baixo";
    }

    private static string ResolveDecisionPriority(int score)
    {
        if (score >= 75) return "Imediata";
        if (score >= 50) return "Alta";
        if (score >= 25) return "Direcionada";
        return "Monitoramento";
    }

    private static string ResolveRecommendedAction(int score, decimal forecast6h, decimal forecast24h, decimal tideCurrent, int alagamentos24h)
    {
        if (score >= 75 || alagamentos24h >= 2)
        {
            return "Acionar protocolo preventivo, proteger ativos expostos e intensificar resposta de campo.";
        }

        if (score >= 50 || forecast6h >= 5m || forecast24h >= 15m)
        {
            return "Reforcar monitoramento hiperlocal, revisar corredores sensiveis e preposicionar equipes.";
        }

        if (score >= 25 || tideCurrent >= 2.0m)
        {
            return "Manter observacao direcionada e triagem operacional ativa.";
        }

        return "Seguir monitoramento preventivo e consolidacao das fontes.";
    }

    private static string ResolveCriticalWindow(decimal forecast6h, decimal forecast12h, decimal forecast24h, DateTime peakHourlyAtUtc)
    {
        if (forecast6h >= 4m)
        {
            return $"Proximas 6h | pico previsto {peakHourlyAtUtc.ToLocalTime():HH\\h}";
        }

        if (forecast12h > forecast6h)
        {
            return "6h a 12h";
        }

        if (forecast24h > forecast12h)
        {
            return "12h a 24h";
        }

        return "Sem janela critica destacada";
    }

    private static int ResolveFreshnessMinutes(DateTime timestampUtc)
    {
        return Math.Max(0, (int)Math.Round(DateTime.UtcNow.Subtract(timestampUtc).TotalMinutes));
    }

    private static string ResolveFreshnessLabel(int freshnessMinutes, bool isFresh)
    {
        if (!isFresh)
        {
            return "Desatualizado";
        }

        if (freshnessMinutes <= 15)
        {
            return "Muito recente";
        }

        if (freshnessMinutes <= 60)
        {
            return "Recente";
        }

        return "Monitorado";
    }

    private static decimal ScoreLinear(decimal value, decimal floor, decimal ceiling, decimal maxScore)
    {
        if (value <= floor)
        {
            return 0m;
        }

        if (value >= ceiling)
        {
            return maxScore;
        }

        var ratio = (value - floor) / (ceiling - floor);
        return decimal.Round(ratio * maxScore, 2);
    }

    private static DateTime Max(DateTime first, DateTime second) => first >= second ? first : second;

    private static DateTime Max(DateTime first, DateTime second, DateTime third) => Max(Max(first, second), third);

    private static decimal CalculateDistanceKm(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
    {
        const double earthRadiusKm = 6371.0;
        var dLat = DegreesToRadians((double)(lat2 - lat1));
        var dLon = DegreesToRadians((double)(lon2 - lon1));
        var latitude1 = DegreesToRadians((double)lat1);
        var latitude2 = DegreesToRadians((double)lat2);

        var sinLat = Math.Sin(dLat / 2);
        var sinLon = Math.Sin(dLon / 2);

        var a = sinLat * sinLat +
                Math.Cos(latitude1) * Math.Cos(latitude2) *
                sinLon * sinLon;
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return (decimal)(earthRadiusKm * c);
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180d;

    private static string CompactToken(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "n-a";
        }

        var compact = value
            .Replace('|', '-')
            .Replace(',', ';')
            .Replace(':', '-')
            .Trim();

        return compact.Length > 48 ? compact[..48] : compact;
    }

    private sealed class DataFusionTraceState
    {
        public string Signature { get; set; } = string.Empty;

        public DateTime LoggedAtUtc { get; set; }
    }
}
