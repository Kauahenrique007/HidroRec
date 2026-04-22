namespace HidroRec.Backend.Application.DTOs.Previsoes;

public sealed class PrevisaoResumoDto
{
    public string SituacaoAtual { get; set; } = string.Empty;

    public string LeituraOficial { get; set; } = string.Empty;

    public string Recomendacao { get; set; } = string.Empty;

    public DateTime AtualizadoEm { get; set; }

    public string Janela24h { get; set; } = string.Empty;

    public IReadOnlyCollection<PrevisaoCardDto> Indicadores { get; set; } = Array.Empty<PrevisaoCardDto>();

    public IReadOnlyCollection<JanelaCriticaDto> PeriodosCriticos { get; set; } = Array.Empty<JanelaCriticaDto>();
}
