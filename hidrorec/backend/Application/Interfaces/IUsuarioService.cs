using HidroRec.Backend.Application.DTOs.Usuarios;
using HidroRec.Backend.Application.DTOs.Common;
using HidroRec.Backend.Application.DTOs.Reportes;

namespace HidroRec.Backend.Application.Interfaces;

public interface IUsuarioService
{
    Task<IReadOnlyCollection<UsuarioDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<UsuarioDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<UsuarioDto> UpdateAsync(Guid id, UpdateUsuarioRequestDto request, CancellationToken cancellationToken);

    Task<UsuarioDto> UpdateOwnAsync(Guid id, UpdateOwnUsuarioRequestDto request, CancellationToken cancellationToken);

    Task<PagedResultDto<ReporteAdminItemDto>> GetOwnReportesAsync(Guid usuarioId, int page, int pageSize, CancellationToken cancellationToken);
}
