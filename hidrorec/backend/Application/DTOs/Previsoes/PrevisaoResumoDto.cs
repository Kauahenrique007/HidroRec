namespace HidroRec.Backend.Application.DTOs.Previsoes;

public sealed class PrevisaoResumoDto
{
    public IReadOnlyCollection<PrevisaoCardDto> Indicadores { get; set; } = Array.Empty<PrevisaoCardDto>();

    public IReadOnlyCollection<JanelaCriticaDto> PeriodosCriticos { get; set; } = Array.Empty<JanelaCriticaDto>();
}
