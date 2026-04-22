namespace HidroRec.Backend.Application.DTOs.Dashboard;

public sealed class GovernancaOperacionalDto
{
    public DateTime AtualizadoEm { get; set; }

    public string ConfiabilidadeLeitura { get; set; } = string.Empty;

    public string CoberturaTerritorial { get; set; } = string.Empty;

    public string FontesAtivas { get; set; } = string.Empty;

    public string DiretrizExecutiva { get; set; } = string.Empty;
}
