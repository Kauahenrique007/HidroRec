using HidroRec.Backend.Application.DTOs.Admin;
using HidroRec.Backend.Application.DTOs.Common;
using HidroRec.Backend.Application.DTOs.Reportes;
using HidroRec.Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HidroRec.Backend.Controllers;

[ApiController]
[Authorize(Policy = "GestorOuAdmin")]
[Route("api/admin")]
public sealed class AdminController(IAdminService adminService) : ControllerBase
{
    [HttpGet("reportes")]
    public async Task<ActionResult<ApiResponse<PagedResultDto<ReporteAdminItemDto>>>> GetReportes(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? bairro = null,
        CancellationToken cancellationToken = default)
    {
        var result = await adminService.GetReportesAsync(page, pageSize, status, bairro, cancellationToken);
        return Ok(ApiResponse<PagedResultDto<ReporteAdminItemDto>>.Ok(result));
    }

    [HttpGet("metricas")]
    public async Task<ActionResult<ApiResponse<AdminMetricasDto>>> GetMetricas(CancellationToken cancellationToken)
    {
        var result = await adminService.GetMetricasAsync(cancellationToken);
        return Ok(ApiResponse<AdminMetricasDto>.Ok(result));
    }

    [HttpGet("auditoria")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<AuditoriaDto>>>> GetAuditoria(CancellationToken cancellationToken)
    {
        var result = await adminService.GetAuditoriaAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<AuditoriaDto>>.Ok(result));
    }

    [HttpGet("logs")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<LogSistemaDto>>>> GetLogs(CancellationToken cancellationToken)
    {
        var result = await adminService.GetLogsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<LogSistemaDto>>.Ok(result));
    }
}
