using HidroRec.Backend.Application.DTOs.Alertas;

namespace HidroRec.Backend.Application.Interfaces;

public interface IAlertaService
{
    Task<IReadOnlyCollection<AlertaDto>> GetAllAsync(bool somenteAtivos, string? criticidade, CancellationToken cancellationToken);

    Task<AlertaDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
