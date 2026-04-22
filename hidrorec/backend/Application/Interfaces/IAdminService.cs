using HidroRec.Backend.Application.DTOs.Admin;
using HidroRec.Backend.Application.DTOs.Common;
using HidroRec.Backend.Application.DTOs.Reportes;

namespace HidroRec.Backend.Application.Interfaces;

public interface IAdminService
{
    Task<PagedResultDto<ReporteAdminItemDto>> GetReportesAsync(int page, int pageSize, string? status, string? bairro, CancellationToken cancellationToken);

    Task<AdminMetricasDto> GetMetricasAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<AuditoriaDto>> GetAuditoriaAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<LogSistemaDto>> GetLogsAsync(string? contexto, string? evento, CancellationToken cancellationToken);
}
