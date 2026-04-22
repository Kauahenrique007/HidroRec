namespace HidroRec.Backend.Application.DTOs.DataFusion;

public sealed class OperationalDataFusionSnapshotDto
{
    public string TraceId { get; set; } = string.Empty;

    public DateTime GeneratedAtUtc { get; set; }

    public decimal DataQualityScore { get; set; }

    public LocationContextDto Location { get; set; } = new();

    public ClimateFusionDto Climate { get; set; } = new();

    public TideFusionDto Tide { get; set; } = new();

    public HistoricalContextDto History { get; set; } = new();

    public RiskAssessmentDto Risk { get; set; } = new();

    public IReadOnlyCollection<DataSourceStatusDto> Sources { get; set; } = Array.Empty<DataSourceStatusDto>();

    public IReadOnlyCollection<string> Warnings { get; set; } = Array.Empty<string>();
}
