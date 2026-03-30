using HidroRec.Backend.Application.DTOs.Auth;
using HidroRec.Backend.Application.DTOs.Common;
using HidroRec.Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HidroRec.Backend.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [EnableRateLimiting("AuthPolicy")]
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register(RegisterRequestDto request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);
        return Created(string.Empty, ApiResponse<AuthResponseDto>.Ok(result, "Usuario registrado com sucesso."));
    }

    [EnableRateLimiting("AuthPolicy")]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(LoginRequestDto request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login realizado com sucesso."));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UsuarioResumoDto>>> Me(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userId, out var currentUserId))
        {
            return Unauthorized(new
            {
                success = false,
                message = "Sessao invalida."
            });
        }

        var result = await authService.GetCurrentUserAsync(currentUserId, cancellationToken);
        return Ok(ApiResponse<UsuarioResumoDto>.Ok(result));
    }
}
