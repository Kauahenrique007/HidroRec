using System.Globalization;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;
using HidroRec.Backend.Application.Interfaces;
using HidroRec.Backend.Configurations;
using Microsoft.Extensions.Options;

namespace HidroRec.Backend.Infrastructure.ExternalServices;

public sealed class WeatherService(
    HttpClient httpClient,
    IOptions<ExternalDataOptions> externalOptions,
    IExternalDataCache cache,
    ILogger<WeatherService> logger) : IWeatherService
{
    private const string ObservedCacheKey = "fusion:weather:observed";
    private const string ForecastCacheKey = "fusion:weather:forecast";

    public async Task<(decimal VolumeMmHora, string Status, string Descricao)> GetCurrentRainAsync(CancellationToken cancellationToken)
    {
        var observed = await GetObservedWeatherAsync(cancellationToken);
        return (observed.Rain24hMm, ResolveRainStatus(observed.Rain24hMm), observed.Summary);
    }

    public async Task<RainForecastOutlook> GetShortTermForecastAsync(CancellationToken cancellationToken)
    {
        var forecast = await GetForecastWeatherAsync(cancellationToken);
        return new RainForecastOutlook(
            forecast.Forecast6hMm,
            forecast.Forecast12hMm,
            forecast.Forecast24hMm,
            forecast.PeakHourlyMm,
            forecast.PeakHourlyAtUtc,
            forecast.MaxProbabilityPercent,
            forecast.IsFresh ? ResolveForecastStatus(forecast.Forecast6hMm, forecast.Forecast24hMm, forecast.PeakHourlyMm, forecast.MaxProbabilityPercent) : "Monitorada",
            forecast.Summary);
    }

    public async Task<WeatherObservedReading> GetObservedWeatherAsync(CancellationToken cancellationToken)
    {
        var apac = await TryGetApacObservedAsync(cancellationToken);
        if (apac is not null)
        {
            cache.Set(ObservedCacheKey, apac);
            return apac;
        }

        if (cache.TryGet<WeatherObservedReading>(ObservedCacheKey, out var cached) && cached is not null)
        {
            logger.LogWarning("Usando ultimo dado observado valido em cache para chuva.");
            return cached with
            {
                Reliability = Math.Round(Math.Max(0.45m, cached.Reliability - 0.15m), 2),
                Summary = $"{cached.Summary} | cache de ultimo dado valido."
            };
        }

        throw new InvalidOperationException("Nenhuma leitura observada de chuva disponivel nas fontes conectadas.");
    }

    public async Task<WeatherForecastReading> GetForecastWeatherAsync(CancellationToken cancellationToken)
    {
        var options = externalOptions.Value.OpenMeteo;
        var query = string.Create(
            CultureInfo.InvariantCulture,
            $"/v1/forecast?latitude={options.Latitude}&longitude={options.Longitude}&current=temperature_2m,precipitation,rain&hourly=precipitation,precipitation_probability,temperature_2m&forecast_hours={options.ForecastHours}&timezone={Uri.EscapeDataString(options.Timezone)}");
        var requestUri = new Uri(new Uri(options.BaseUrl.TrimEnd('/')), query);
        var fusionOptions = externalOptions.Value.DataFusion;

        try
        {
            var response = await httpClient.GetFromJsonAsync<OpenMeteoForecastResponse>(requestUri, cancellationToken);
            var hourly = response?.Hourly;
            var current = response?.Current;
            if (hourly is null || hourly.Time.Count == 0 || hourly.Precipitation.Count == 0 || current is null)
            {
                throw new InvalidOperationException("Resposta horaria vazia da previsao.");
            }

            var availableHours = Math.Min(hourly.Time.Count, hourly.Precipitation.Count);
            var acumulado6h = SumPrecipitation(hourly.Precipitation, Math.Min(6, availableHours));
            var acumulado12h = SumPrecipitation(hourly.Precipitation, Math.Min(12, availableHours));
            var acumulado24h = SumPrecipitation(hourly.Precipitation, Math.Min(24, availableHours));

            var peakIndex = hourly.Precipitation
                .Select((value, index) => new { value, index })
                .OrderByDescending(item => item.value)
                .First().index;

            var picoHorario = Convert.ToDecimal(hourly.Precipitation[peakIndex], CultureInfo.InvariantCulture);
            var picoHorarioEm = DateTime.TryParse(hourly.Time[peakIndex], out var parsedAt)
                ? parsedAt
                : DateTime.UtcNow.AddHours(1);

            var probabilidadeMaxima = hourly.PrecipitationProbability.Count == 0
                ? 0
                : hourly.PrecipitationProbability.Take(Math.Min(24, hourly.PrecipitationProbability.Count)).Max();

            var generatedAt = ParseDateTimeLocal(current.Time) ?? DateTime.UtcNow;
            var isFresh = DateTime.UtcNow.Subtract(generatedAt.ToUniversalTime()).TotalHours <= fusionOptions.MaxForecastAgeHours;
            var reliability = AdjustReliability(fusionOptions.OpenMeteoForecastReliability, generatedAt, fusionOptions.MaxForecastAgeHours);
            var descricao = $"Open-Meteo | chuva agora {Convert.ToDecimal(current.Precipitation ?? 0d, CultureInfo.InvariantCulture):0.0} mm | temp {Convert.ToDecimal(current.Temperature2M ?? 0d, CultureInfo.InvariantCulture):0.0} C | prox. 6h {acumulado6h:0.0} mm | prox. 24h {acumulado24h:0.0} mm | pico {picoHorario:0.0} mm/h | probabilidade maxima {probabilidadeMaxima}%.";

            var forecast = new WeatherForecastReading(
                acumulado6h,
                acumulado12h,
                acumulado24h,
                picoHorario,
                generatedAt.ToUniversalTime(),
                picoHorarioEm.ToUniversalTime(),
                probabilidadeMaxima,
                reliability,
                isFresh,
                descricao);

            cache.Set(ForecastCacheKey, forecast);
            return forecast;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao consultar previsao horaria externa.");
            if (cache.TryGet<WeatherForecastReading>(ForecastCacheKey, out var cached) && cached is not null)
            {
                return cached with
                {
                    Reliability = Math.Round(Math.Max(0.45m, cached.Reliability - 0.12m), 2),
                    Summary = $"{cached.Summary} | cache de ultimo dado valido."
                };
            }

            throw new InvalidOperationException("Nenhuma previsao horaria disponivel nas fontes conectadas.", ex);
        }
    }

    private async Task<WeatherObservedReading?> TryGetApacObservedAsync(CancellationToken cancellationToken)
    {
        var options = externalOptions.Value.Apac;
        var openMeteo = externalOptions.Value.OpenMeteo;
        var fusionOptions = externalOptions.Value.DataFusion;
        var municipality = Uri.EscapeDataString(options.RecifeMunicipalityName);
        var apacQuery = $"{options.RainMunicipality24hPath}?where=nm_mun%20%3D%20%27{municipality}%27&outFields=nm_mun,horas_24,ultima_leitura_data_hora&returnGeometry=false&f=json";
        var apacRequestUri = new Uri(new Uri(options.BaseUrl.TrimEnd('/')), apacQuery);
        var meteoQuery = string.Create(
            CultureInfo.InvariantCulture,
            $"/v1/forecast?latitude={openMeteo.Latitude}&longitude={openMeteo.Longitude}&current=temperature_2m,precipitation,rain&timezone={Uri.EscapeDataString(openMeteo.Timezone)}");
        var meteoRequestUri = new Uri(new Uri(openMeteo.BaseUrl.TrimEnd('/')), meteoQuery);

        var apacTask = httpClient.GetFromJsonAsync<ApacFeatureCollection>(apacRequestUri, cancellationToken);
        var meteoTask = httpClient.GetFromJsonAsync<OpenMeteoForecastResponse>(meteoRequestUri, cancellationToken);
        await Task.WhenAll(apacTask, meteoTask);

        var feature = apacTask.Result?.Features?.FirstOrDefault();
        var current = meteoTask.Result?.Current;
        if (feature?.Attributes?.Horas24 is null || current is null)
        {
            return null;
        }

        var rain24h = Convert.ToDecimal(feature.Attributes.Horas24.Value, CultureInfo.InvariantCulture);
        var observedAt = ParseApacTimestamp(feature.Attributes.UltimaLeituraDataHora) ?? DateTime.UtcNow;
        var currentAt = ParseDateTimeLocal(current.Time) ?? DateTime.UtcNow;
        var currentRain = Convert.ToDecimal(current.Precipitation ?? current.Rain ?? 0d, CultureInfo.InvariantCulture);
        var temperature = Convert.ToDecimal(current.Temperature2M ?? 0d, CultureInfo.InvariantCulture);
        var freshestAt = observedAt > currentAt ? observedAt : currentAt;
        var reliability = Math.Round(
            (AdjustReliability(fusionOptions.ApacReliability, observedAt, fusionOptions.MaxObservedRainAgeHours)
            + AdjustReliability(fusionOptions.OpenMeteoObservedReliability, currentAt, fusionOptions.MaxForecastAgeHours)) / 2m, 2);
        var isFresh = DateTime.UtcNow.Subtract(observedAt.ToUniversalTime()).TotalHours <= fusionOptions.MaxObservedRainAgeHours;
        var summary = $"APAC {rain24h:0.0} mm/24h | leitura {observedAt:yyyy-MM-dd HH:mm} | Open-Meteo agora {currentRain:0.0} mm e {temperature:0.0} C em {currentAt:yyyy-MM-dd HH:mm}.";

        return new WeatherObservedReading(
            rain24h,
            currentRain,
            temperature,
            observedAt.ToUniversalTime(),
            currentAt.ToUniversalTime(),
            reliability,
            isFresh,
            summary);
    }

    private static decimal SumPrecipitation(IReadOnlyList<double> precipitation, int take) =>
        Enumerable.Range(0, take)
            .Select(index => Convert.ToDecimal(precipitation[index], CultureInfo.InvariantCulture))
            .Sum();

    private static DateTime? ParseApacTimestamp(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var match = Regex.Match(raw, @"\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}");
        if (!match.Success)
        {
            return null;
        }

        return ParseDateTimeLocal(match.Value);
    }

    private static DateTime? ParseDateTimeLocal(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var formats = new[]
        {
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-ddTHH:mm",
            "yyyy-MM-ddTHH:mm:ss"
        };

        if (DateTime.TryParseExact(raw, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed))
        {
            return DateTime.SpecifyKind(parsed, DateTimeKind.Local);
        }

        if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out parsed))
        {
            return DateTime.SpecifyKind(parsed, DateTimeKind.Local);
        }

        return null;
    }

    private static decimal AdjustReliability(decimal baseReliability, DateTime referenceTime, int freshnessHours)
    {
        var ageHours = Math.Max(0, (decimal)DateTime.UtcNow.Subtract(referenceTime.ToUniversalTime()).TotalHours);
        var freshnessWindow = Math.Max(1, freshnessHours);
        var freshnessFactor = Math.Max(0.45m, 1m - (ageHours / freshnessWindow) * 0.35m);
        return Math.Round(baseReliability * freshnessFactor, 2);
    }

    private static string ResolveRainStatus(decimal rain24h)
    {
        if (rain24h >= 100m) return "Extrema";
        if (rain24h >= 50m) return "Muito Forte";
        if (rain24h >= 30m) return "Forte";
        if (rain24h >= 10m) return "Moderada";
        if (rain24h >= 2m) return "Fraca";
        return "Baixa";
    }

    private static string ResolveForecastStatus(decimal accumulated6Hours, decimal accumulated24Hours, decimal peakHourly, int probabilityMax)
    {
        if (accumulated24Hours >= 20m || accumulated6Hours >= 8m || peakHourly >= 4m)
        {
            return "Elevada";
        }

        if (accumulated24Hours >= 10m || accumulated6Hours >= 4m || peakHourly >= 2m || probabilityMax >= 80)
        {
            return "Atencao";
        }

        if (accumulated24Hours >= 4m || accumulated6Hours >= 2m || probabilityMax >= 60)
        {
            return "Monitorada";
        }

        return "Baixa";
    }

    private sealed class ApacFeatureCollection
    {
        [JsonPropertyName("features")]
        public IReadOnlyCollection<ApacFeature> Features { get; set; } = Array.Empty<ApacFeature>();
    }

    private sealed class ApacFeature
    {
        [JsonPropertyName("attributes")]
        public ApacAttributes? Attributes { get; set; }
    }

    private sealed class ApacAttributes
    {
        [JsonPropertyName("horas_24")]
        public double? Horas24 { get; set; }

        [JsonPropertyName("ultima_leitura_data_hora")]
        public string? UltimaLeituraDataHora { get; set; }
    }

    private sealed class OpenMeteoForecastResponse
    {
        [JsonPropertyName("current")]
        public OpenMeteoCurrentResponse? Current { get; set; }

        [JsonPropertyName("hourly")]
        public OpenMeteoHourlyResponse? Hourly { get; set; }
    }

    private sealed class OpenMeteoCurrentResponse
    {
        [JsonPropertyName("time")]
        public string? Time { get; set; }

        [JsonPropertyName("temperature_2m")]
        public double? Temperature2M { get; set; }

        [JsonPropertyName("precipitation")]
        public double? Precipitation { get; set; }

        [JsonPropertyName("rain")]
        public double? Rain { get; set; }
    }

    private sealed class OpenMeteoHourlyResponse
    {
        [JsonPropertyName("time")]
        public IReadOnlyList<string> Time { get; set; } = Array.Empty<string>();

        [JsonPropertyName("precipitation")]
        public IReadOnlyList<double> Precipitation { get; set; } = Array.Empty<double>();

        [JsonPropertyName("precipitation_probability")]
        public IReadOnlyList<int> PrecipitationProbability { get; set; } = Array.Empty<int>();
    }
}
