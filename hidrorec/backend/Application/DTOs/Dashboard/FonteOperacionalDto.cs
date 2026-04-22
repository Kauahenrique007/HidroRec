namespace HidroRec.Backend.Application.DTOs.Dashboard;

public sealed class FonteOperacionalDto
{
    public string Nome { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Detalhe { get; set; } = string.Empty;

    public DateTime AtualizadoEm { get; set; }
}
