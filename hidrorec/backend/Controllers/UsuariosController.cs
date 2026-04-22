using HidroRec.Backend.Application.DTOs.Common;
using HidroRec.Backend.Application.DTOs.Reportes;
using HidroRec.Backend.Application.DTOs.Usuarios;
using HidroRec.Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HidroRec.Backend.Controllers;

[ApiController]
[Route("api/usuarios")]
public sealed class UsuariosController(IUsuarioService usuarioService) : ControllerBase
{
    [Authorize]
    [HttpGet("me/reportes")]
    public async Task<ActionResult<ApiResponse<PagedResultDto<ReporteAdminItemDto>>>> GetOwnReportes(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new
            {
                success = false,
                message = "Sessao invalida."
            });
        }

        var result = await usuarioService.GetOwnReportesAsync(userId.Value, page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResultDto<ReporteAdminItemDto>>.Ok(result));
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> UpdateOwn(UpdateOwnUsuarioRequestDto request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new
            {
                success = false,
                message = "Sessao invalida."
            });
        }

        var result = await usuarioService.UpdateOwnAsync(userId.Value, request, cancellationToken);
        return Ok(ApiResponse<UsuarioDto>.Ok(result, "Perfil atualizado com sucesso."));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<UsuarioDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await usuarioService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<UsuarioDto>>.Ok(result));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await usuarioService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<UsuarioDto>.Ok(result));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> Update(Guid id, UpdateUsuarioRequestDto request, CancellationToken cancellationToken)
    {
        var result = await usuarioService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<UsuarioDto>.Ok(result, "Usuario atualizado com sucesso."));
    }

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
            ?? User.FindFirst("sub")?.Value;

        return Guid.TryParse(value, out var id) ? id : null;
    }
}
