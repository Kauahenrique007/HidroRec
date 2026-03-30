using HidroRec.Backend.Application.DTOs.Alertas;
using HidroRec.Backend.Application.DTOs.Common;
using HidroRec.Backend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HidroRec.Backend.Controllers;

[ApiController]
[Route("api/alertas")]
public sealed class AlertasController(IAlertaService alertaService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<AlertaDto>>>> GetAll([FromQuery] string? criticidade, CancellationToken cancellationToken)
    {
        var result = await alertaService.GetAllAsync(false, criticidade, cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<AlertaDto>>.Ok(result));
    }

    [HttpGet("ativos")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<AlertaDto>>>> GetAtivos([FromQuery] string? criticidade, CancellationToken cancellationToken)
    {
        var result = await alertaService.GetAllAsync(true, criticidade, cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<AlertaDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AlertaDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await alertaService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<AlertaDto>.Ok(result));
    }
}
