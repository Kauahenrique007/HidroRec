namespace HidroRec.Backend.Application.DTOs.Dashboard;

public sealed class AreaCriticaDto
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Organizacao { get; set; } = string.Empty;

    public string Bairro { get; set; } = string.Empty;

    public string Regiao { get; set; } = string.Empty;

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public int ScoreRisco { get; set; }

    public string NivelRisco { get; set; } = string.Empty;

    public string Tendencia { get; set; } = string.Empty;

    public string Prioridade { get; set; } = string.Empty;

    public int ReportesRecentes { get; set; }

    public int AtivosExpostos { get; set; }

    public string JanelaCritica { get; set; } = string.Empty;

    public string AcaoSugerida { get; set; } = string.Empty;

    public string LeituraExplicavel { get; set; } = string.Empty;
}
