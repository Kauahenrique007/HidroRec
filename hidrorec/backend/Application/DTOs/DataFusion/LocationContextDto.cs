namespace HidroRec.Backend.Application.DTOs.DataFusion;

public sealed class LocationContextDto
{
    public int? AreaMonitoradaId { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public string Bairro { get; set; } = string.Empty;

    public string Regiao { get; set; } = string.Empty;

    public string AreaMonitorada { get; set; } = string.Empty;

    public string ResolutionMethod { get; set; } = string.Empty;

    public decimal? DistanceKm { get; set; }

    public string CoverageContext { get; set; } = string.Empty;
}
