using HidroRec.Backend.Application.Interfaces;
using HidroRec.Backend.Configurations;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace HidroRec.Backend.Infrastructure.ExternalServices;

public sealed class TideService(
    HttpClient httpClient,
    IOptions<ExternalDataOptions> externalOptions,
    IExternalDataCache cache,
    ILogger<TideService> logger) : ITideService
{
    private const string TideCacheKey = "fusion:tide:current";

    public Task<(decimal AlturaMetros, string Status, string Descricao)> GetCurrentTideAsync(CancellationToken cancellationToken)
    {
        return GetCurrentTideCoreAsync(cancellationToken);
    }

    public async Task<TideReading> GetTideReadingAsync(CancellationToken cancellationToken)
    {
        var options = externalOptions.Value;
        var nowLocal = DateTime.Now;
        var tideEvents = await TryGetTideEventsAsync(nowLocal, cancellationToken);
        if (tideEvents.Count > 1)
        {
            var reading = BuildReading(tideEvents, nowLocal, options);
            cache.Set(TideCacheKey, reading);
            return reading;
        }

        if (cache.TryGet<TideReading>(TideCacheKey, out var cached) && cached is not null)
        {
            logger.LogWarning("Usando ultimo dado valido de mare em cache.");
            return cached with
            {
                Reliability = Math.Round(Math.Max(0.45m, cached.Reliability - 0.15m), 2),
                Summary = $"{cached.Summary} | cache de ultimo dado valido."
            };
        }

        throw new InvalidOperationException("Nenhuma leitura real de mare disponivel.");
    }

    private async Task<(decimal AlturaMetros, string Status, string Descricao)> GetCurrentTideCoreAsync(CancellationToken cancellationToken)
    {
        var tide = await GetTideReadingAsync(cancellationToken);
        return (tide.CurrentLevelMeters, tide.Trend, tide.Summary);
    }

    private async Task<List<TideEvent>> TryGetTideEventsAsync(DateTime nowLocal, CancellationToken cancellationToken)
    {
        var options = externalOptions.Value.TideApi;
        var baseUri = options.BaseUrl.TrimEnd('/');
        var today = nowLocal.Date;
        var tomorrow = today.AddDays(1);
        var requests = new List<Uri>();

        requests.Add(new Uri($"{baseUri}/api/v2/tabua-mare/{options.HarborId}/{today.Month}/[{today.Day}]"));

        if (tomorrow.Month == today.Month)
        {
            requests[0] = new Uri($"{baseUri}/api/v2/tabua-mare/{options.HarborId}/{today.Month}/[{today.Day},{tomorrow.Day}]");
        }
        else
        {
            requests.Add(new Uri($"{baseUri}/api/v2/tabua-mare/{options.HarborId}/{tomorrow.Month}/[{tomorrow.Day}]"));
        }

        var results = new List<TideEvent>();

        foreach (var request in requests)
        {
            try
            {
                var response = await httpClient.GetFromJsonAsync<TideApiResponse>(request, cancellationToken);
                var harbor = response?.Data?.FirstOrDefault();
                if (harbor?.Months is null)
                {
                    continue;
                }

                foreach (var month in harbor.Months)
                {
                    foreach (var day in month.Days)
                    {
                        foreach (var hour in day.Hours)
                        {
                            if (!TimeSpan.TryParse(hour.Hour, CultureInfo.InvariantCulture, out var eventTime))
                            {
                                continue;
                            }

                            var eventDateTime = new DateTime(harbor.Year, month.Month, day.Day, eventTime.Hours, eventTime.Minutes, eventTime.Seconds, DateTimeKind.Local);
                            results.Add(new TideEvent(eventDateTime, Convert.ToDecimal(hour.Level, CultureInfo.InvariantCulture)));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Falha ao consultar tábua de marés.");
            }
        }

        return results
            .OrderBy(item => item.AtLocal)
            .ToList();
    }

    private TideReading BuildReading(IReadOnlyList<TideEvent> events, DateTime nowLocal, ExternalDataOptions options)
    {
        var fusion = options.DataFusion;
        var harborId = options.TideApi.HarborId;
        var previous = events.LastOrDefault(item => item.AtLocal <= nowLocal);
        var next = events.FirstOrDefault(item => item.AtLocal >= nowLocal);

        if (previous == default)
        {
            previous = events.First();
        }

        if (next == default)
        {
            next = events.Last();
        }

        var currentLevel = InterpolateLevel(previous, next, nowLocal);
        var trend = next.LevelMeters >= previous.LevelMeters ? "Subindo" : "Descendo";
        var next24hMax = events
            .Where(item => item.AtLocal >= nowLocal && item.AtLocal <= nowLocal.AddHours(24))
            .Select(item => item.LevelMeters)
            .DefaultIfEmpty(currentLevel)
            .Max();
        var reliability = Math.Round(options.DataFusion.TideTableReliability, 2);
        var summary = $"Tábua de marés {harborId.ToUpperInvariant()} | nivel estimado {currentLevel:0.00} m | proximo extremo {next.LevelMeters:0.00} m às {next.AtLocal:HH:mm}.";

        return new TideReading(
            decimal.Round(currentLevel, 2),
            decimal.Round(next.LevelMeters, 2),
            nowLocal.ToUniversalTime(),
            next.AtLocal.ToUniversalTime(),
            decimal.Round(next24hMax, 2),
            trend,
            "Porto do Recife (DHN)",
            reliability,
            true,
            summary);
    }

    private static decimal InterpolateLevel(TideEvent previous, TideEvent next, DateTime nowLocal)
    {
        if (previous.AtLocal == next.AtLocal)
        {
            return previous.LevelMeters;
        }

        var totalMinutes = (decimal)(next.AtLocal - previous.AtLocal).TotalMinutes;
        var elapsedMinutes = (decimal)(nowLocal - previous.AtLocal).TotalMinutes;
        var ratio = totalMinutes == 0 ? 0 : Math.Clamp(elapsedMinutes / totalMinutes, 0, 1);
        return previous.LevelMeters + ((next.LevelMeters - previous.LevelMeters) * ratio);
    }

    private sealed record TideEvent(DateTime AtLocal, decimal LevelMeters);

    private sealed class TideApiResponse
    {
        [JsonPropertyName("data")]
        public IReadOnlyCollection<TideHarborDto> Data { get; set; } = Array.Empty<TideHarborDto>();
    }

    private sealed class TideHarborDto
    {
        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("months")]
        public IReadOnlyCollection<TideMonthDto> Months { get; set; } = Array.Empty<TideMonthDto>();
    }

    private sealed class TideMonthDto
    {
        [JsonPropertyName("month")]
        public int Month { get; set; }

        [JsonPropertyName("days")]
        public IReadOnlyCollection<TideDayDto> Days { get; set; } = Array.Empty<TideDayDto>();
    }

    private sealed class TideDayDto
    {
        [JsonPropertyName("day")]
        public int Day { get; set; }

        [JsonPropertyName("hours")]
        public IReadOnlyCollection<TideHourDto> Hours { get; set; } = Array.Empty<TideHourDto>();
    }

    private sealed class TideHourDto
    {
        [JsonPropertyName("hour")]
        public string Hour { get; set; } = string.Empty;

        [JsonPropertyName("level")]
        public double Level { get; set; }
    }
}
