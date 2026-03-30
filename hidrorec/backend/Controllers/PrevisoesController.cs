using HidroRec.Backend.Application.DTOs.Common;
using HidroRec.Backend.Application.DTOs.Previsoes;
using HidroRec.Backend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HidroRec.Backend.Controllers;

[ApiController]
[Route("api/previsoes")]
public sealed class PrevisoesController(IPrevisaoService previsaoService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PrevisaoResumoDto>>> GetResumo(CancellationToken cancellationToken)
    {
        var result = await previsaoService.GetResumoAsync(cancellationToken);
        return Ok(ApiResponse<PrevisaoResumoDto>.Ok(result));
    }

    [HttpGet("chuva")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<PrevisaoCardDto>>>> GetChuva(CancellationToken cancellationToken)
    {
        var result = await previsaoService.GetChuvaAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<PrevisaoCardDto>>.Ok(result));
    }

    [HttpGet("mare")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<PrevisaoCardDto>>>> GetMare(CancellationToken cancellationToken)
    {
        var result = await previsaoService.GetMareAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<PrevisaoCardDto>>.Ok(result));
    }
}
