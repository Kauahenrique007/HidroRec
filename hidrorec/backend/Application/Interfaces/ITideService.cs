namespace HidroRec.Backend.Application.Interfaces;

public interface ITideService
{
    Task<(decimal AlturaMetros, string Status, string Descricao)> GetCurrentTideAsync(CancellationToken cancellationToken);

    Task<TideReading> GetTideReadingAsync(CancellationToken cancellationToken);
}

public sealed record TideReading(
    decimal CurrentLevelMeters,
    decimal NextExtremeLevelMeters,
    DateTime UpdatedAtUtc,
    DateTime NextExtremeAtUtc,
    decimal Max24hLevelMeters,
    string Trend,
    string HarborName,
    decimal Reliability,
    bool IsFresh,
    string Summary);
