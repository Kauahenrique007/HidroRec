using HidroRec.Backend.Application.DTOs.Dashboard;

namespace HidroRec.Backend.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardResumoDto> GetResumoAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<IndicadorCardDto>> GetIndicadoresAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PontoAtencaoDto>> GetPontosRecentesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<MapaPontoDto>> GetMapaAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PontoAtencaoDto>> GetPontosAtencaoAsync(CancellationToken cancellationToken);
}
