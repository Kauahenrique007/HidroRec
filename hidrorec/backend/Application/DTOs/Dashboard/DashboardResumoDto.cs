namespace HidroRec.Backend.Application.DTOs.Dashboard;

public sealed class DashboardResumoDto
{
    public StatusCidadeDto StatusCidade { get; set; } = new();

    public IndicadoresResumoDto Indicadores { get; set; } = new();

    public GovernancaOperacionalDto Governanca { get; set; } = new();

    public IReadOnlyCollection<MapaPontoDto> Mapa { get; set; } = Array.Empty<MapaPontoDto>();

    public IReadOnlyCollection<PontoAtencaoDto> PontosAtencao { get; set; } = Array.Empty<PontoAtencaoDto>();

    public IReadOnlyCollection<AreaCriticaDto> AreasCriticas { get; set; } = Array.Empty<AreaCriticaDto>();

    public IReadOnlyCollection<AtivoExpostoDto> AtivosExpostos { get; set; } = Array.Empty<AtivoExpostoDto>();

    public IReadOnlyCollection<DecisaoOperacionalDto> DecisoesOperacionais { get; set; } = Array.Empty<DecisaoOperacionalDto>();

    public IReadOnlyCollection<FonteOperacionalDto> Fontes { get; set; } = Array.Empty<FonteOperacionalDto>();

    public string BannerAlerta { get; set; } = string.Empty;
}
