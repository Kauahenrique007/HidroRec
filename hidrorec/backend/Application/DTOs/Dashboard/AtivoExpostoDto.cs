namespace HidroRec.Backend.Application.DTOs.Dashboard;

public sealed class AtivoExpostoDto
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;

    public string Organizacao { get; set; } = string.Empty;

    public string Area { get; set; } = string.Empty;

    public int ScoreRisco { get; set; }

    public string NivelRisco { get; set; } = string.Empty;

    public string StatusOperacional { get; set; } = string.Empty;

    public string AcaoSugerida { get; set; } = string.Empty;
}
