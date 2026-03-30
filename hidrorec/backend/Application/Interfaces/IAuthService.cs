using HidroRec.Backend.Application.DTOs.Auth;

namespace HidroRec.Backend.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken);

    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken);

    Task<UsuarioResumoDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken);
}
