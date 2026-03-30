using HidroRec.Backend.Application.DTOs.Common;
using HidroRec.Backend.Application.DTOs.Reportes;

namespace HidroRec.Backend.Application.Interfaces;

public interface IReporteService
{
    Task<ReporteDto> CreateAsync(CreateReporteRequestDto request, Guid? usuarioId, CancellationToken cancellationToken);

    Task<PagedResultDto<ReporteAdminItemDto>> GetAllAsync(int page, int pageSize, string? status, string? bairro, CancellationToken cancellationToken);

    Task<ReporteDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ReporteDto> UpdateAsync(Guid id, UpdateReporteRequestDto request, CancellationToken cancellationToken);

    Task<ReporteDto> UpdateStatusAsync(Guid id, UpdateReporteStatusRequestDto request, Guid? usuarioId, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, Guid? usuarioId, CancellationToken cancellationToken);
}
