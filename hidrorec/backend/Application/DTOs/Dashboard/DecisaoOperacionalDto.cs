namespace HidroRec.Backend.Application.DTOs.Dashboard;

public sealed class DecisaoOperacionalDto
{
    public string Titulo { get; set; } = string.Empty;

    public string Prioridade { get; set; } = string.Empty;

    public string Area { get; set; } = string.Empty;

    public string JanelaCritica { get; set; } = string.Empty;

    public string Acao { get; set; } = string.Empty;

    public string Fundamentacao { get; set; } = string.Empty;
}
