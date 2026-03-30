using HidroRec.Backend.Application.DTOs.Common;
using HidroRec.Backend.Application.DTOs.Dashboard;
using HidroRec.Backend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HidroRec.Backend.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("resumo")]
    public async Task<ActionResult<ApiResponse<DashboardResumoDto>>> GetResumo(CancellationToken cancellationToken)
    {
        var result = await dashboardService.GetResumoAsync(cancellationToken);
        return Ok(ApiResponse<DashboardResumoDto>.Ok(result));
    }

    [HttpGet("indicadores")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<IndicadorCardDto>>>> GetIndicadores(CancellationToken cancellationToken)
    {
        var result = await dashboardService.GetIndicadoresAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<IndicadorCardDto>>.Ok(result));
    }

    [HttpGet("pontos-recentes")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<PontoAtencaoDto>>>> GetRecentes(CancellationToken cancellationToken)
    {
        var result = await dashboardService.GetPontosRecentesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<PontoAtencaoDto>>.Ok(result));
    }

    [HttpGet("mapa")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<MapaPontoDto>>>> GetMapa(CancellationToken cancellationToken)
    {
        var result = await dashboardService.GetMapaAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<MapaPontoDto>>.Ok(result));
    }

    [HttpGet("pontos-atencao")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<PontoAtencaoDto>>>> GetPontosAtencao(CancellationToken cancellationToken)
    {
        var result = await dashboardService.GetPontosAtencaoAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<PontoAtencaoDto>>.Ok(result));
    }
}
