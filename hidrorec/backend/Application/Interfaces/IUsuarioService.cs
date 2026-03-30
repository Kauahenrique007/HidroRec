using HidroRec.Backend.Application.DTOs.Usuarios;

namespace HidroRec.Backend.Application.Interfaces;

public interface IUsuarioService
{
    Task<IReadOnlyCollection<UsuarioDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<UsuarioDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<UsuarioDto> UpdateAsync(Guid id, UpdateUsuarioRequestDto request, CancellationToken cancellationToken);
}
