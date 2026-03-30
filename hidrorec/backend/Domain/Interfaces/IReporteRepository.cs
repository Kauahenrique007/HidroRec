using HidroRec.Backend.Domain.Entities;

namespace HidroRec.Backend.Domain.Interfaces;

public interface IReporteRepository
{
    IQueryable<Reporte> Query();

    Task AddAsync(Reporte reporte, CancellationToken cancellationToken);
}
