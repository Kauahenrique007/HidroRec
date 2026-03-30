using HidroRec.Backend.Application.DTOs.Common;
using HidroRec.Backend.Application.DTOs.Usuarios;
using HidroRec.Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HidroRec.Backend.Controllers;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/usuarios")]
public sealed class UsuariosController(IUsuarioService usuarioService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<UsuarioDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await usuarioService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<UsuarioDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await usuarioService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<UsuarioDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> Update(Guid id, UpdateUsuarioRequestDto request, CancellationToken cancellationToken)
    {
        var result = await usuarioService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<UsuarioDto>.Ok(result, "Usuario atualizado com sucesso."));
    }
}
