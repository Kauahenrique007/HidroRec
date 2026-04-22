namespace HidroRec.Backend.Application.DTOs.DataFusion;

public sealed class ClimateFusionDto
{
    public decimal RainObserved24hMm { get; set; }

    public decimal CurrentRainMm { get; set; }

    public decimal Forecast6hMm { get; set; }

    public decimal Forecast12hMm { get; set; }

    public decimal Forecast24hMm { get; set; }

    public decimal PeakHourlyRainMm { get; set; }

    public DateTime PeakHourlyAtUtc { get; set; }

    public int MaxProbabilityPercent { get; set; }

    public decimal TemperatureCelsius { get; set; }

    public decimal Reliability { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public string Summary { get; set; } = string.Empty;
}
