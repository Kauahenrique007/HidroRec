namespace HidroRec.Backend.Application.Interfaces;

public interface IWeatherService
{
    Task<(decimal VolumeMmHora, string Status, string Descricao)> GetCurrentRainAsync(CancellationToken cancellationToken);

    Task<RainForecastOutlook> GetShortTermForecastAsync(CancellationToken cancellationToken);

    Task<WeatherObservedReading> GetObservedWeatherAsync(CancellationToken cancellationToken);

    Task<WeatherForecastReading> GetForecastWeatherAsync(CancellationToken cancellationToken);
}

public sealed record RainForecastOutlook(
    decimal Acumulado6hMm,
    decimal Acumulado12hMm,
    decimal Acumulado24hMm,
    decimal PicoHorarioMm,
    DateTime PicoHorarioEm,
    int ProbabilidadeMaxima,
    string Status,
    string Descricao);

public sealed record WeatherObservedReading(
    decimal Rain24hMm,
    decimal CurrentRainMm,
    decimal TemperatureCelsius,
    DateTime ObservedAtUtc,
    DateTime CurrentAtUtc,
    decimal Reliability,
    bool IsFresh,
    string Summary);

public sealed record WeatherForecastReading(
    decimal Forecast6hMm,
    decimal Forecast12hMm,
    decimal Forecast24hMm,
    decimal PeakHourlyMm,
    DateTime GeneratedAtUtc,
    DateTime PeakHourlyAtUtc,
    int MaxProbabilityPercent,
    decimal Reliability,
    bool IsFresh,
    string Summary);
