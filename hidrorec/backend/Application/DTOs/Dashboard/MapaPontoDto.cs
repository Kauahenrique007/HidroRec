namespace HidroRec.Backend.Application.DTOs.Dashboard;

public sealed class MapaPontoDto
{
    public Guid Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Bairro { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Severidade { get; set; } = string.Empty;

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public int ScoreRisco { get; set; }

    public string Categoria { get; set; } = string.Empty;
}
