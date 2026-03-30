namespace HidroRec.Backend.Application.DTOs.Dashboard;

public sealed class DashboardResumoDto
{
    public StatusCidadeDto StatusCidade { get; set; } = new();

    public IndicadoresResumoDto Indicadores { get; set; } = new();

    public IReadOnlyCollection<MapaPontoDto> Mapa { get; set; } = Array.Empty<MapaPontoDto>();

    public IReadOnlyCollection<PontoAtencaoDto> PontosAtencao { get; set; } = Array.Empty<PontoAtencaoDto>();

    public string BannerAlerta { get; set; } = string.Empty;
}
