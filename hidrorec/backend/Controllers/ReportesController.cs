using System.Security.Claims;
using HidroRec.Backend.Application.DTOs.Common;
using HidroRec.Backend.Application.DTOs.Reportes;
using HidroRec.Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HidroRec.Backend.Controllers;

[ApiController]
[Route("api/reportes")]
public sealed class ReportesController(IReporteService reporteService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReporteDto>>> Create(CreateReporteRequestDto request, CancellationToken cancellationToken)
    {
        var result = await reporteService.CreateAsync(request, GetCurrentUserId(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ReporteDto>.Ok(result, "Reporte enviado com sucesso."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResultDto<ReporteAdminItemDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? bairro = null,
        CancellationToken cancellationToken = default)
    {
        var result = await reporteService.GetAllAsync(page, pageSize, status, bairro, cancellationToken);
        return Ok(ApiResponse<PagedResultDto<ReporteAdminItemDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ReporteDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await reporteService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ReporteDto>.Ok(result));
    }

    [Authorize(Policy = "GestorOuAdmin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ReporteDto>>> Update(Guid id, UpdateReporteRequestDto request, CancellationToken cancellationToken)
    {
        var result = await reporteService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<ReporteDto>.Ok(result, "Reporte atualizado com sucesso."));
    }

    [Authorize(Policy = "GestorOuAdmin")]
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<ReporteDto>>> UpdateStatus(Guid id, UpdateReporteStatusRequestDto request, CancellationToken cancellationToken)
    {
        var result = await reporteService.UpdateStatusAsync(id, request, GetCurrentUserId(), cancellationToken);
        return Ok(ApiResponse<ReporteDto>.Ok(result, "Status atualizado com sucesso."));
    }

    [Authorize(Policy = "GestorOuAdmin")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await reporteService.DeleteAsync(id, GetCurrentUserId(), cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Reporte removido com sucesso."));
    }

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue("sub");
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
