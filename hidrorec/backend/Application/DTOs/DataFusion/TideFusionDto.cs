namespace HidroRec.Backend.Application.DTOs.DataFusion;

public sealed class TideFusionDto
{
    public decimal CurrentLevelMeters { get; set; }

    public decimal NextExtremeLevelMeters { get; set; }

    public DateTime NextExtremeAtUtc { get; set; }

    public decimal Max24hLevelMeters { get; set; }

    public string Trend { get; set; } = string.Empty;

    public string HarborName { get; set; } = string.Empty;

    public decimal Reliability { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public string Summary { get; set; } = string.Empty;
}
