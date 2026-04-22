namespace HidroRec.Backend.Application.DTOs.DataFusion;

public sealed class DataSourceStatusDto
{
    public string TraceCode { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime TimestampUtc { get; set; }

    public int FreshnessMinutes { get; set; }

    public decimal Reliability { get; set; }

    public bool IsFresh { get; set; }

    public bool FallbackUsed { get; set; }

    public string FreshnessLabel { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;
}
