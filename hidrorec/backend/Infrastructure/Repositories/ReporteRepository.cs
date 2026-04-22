using HidroRec.Backend.Domain.Entities;
using HidroRec.Backend.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HidroRec.Backend.Infrastructure.Repositories;

public sealed class ReporteRepository(HidroRec.Backend.Infrastructure.Data.HidroRecDbContext context) : IReporteRepository
{
    public IQueryable<Reporte> Query() =>
        context.Reportes
            .AsNoTracking()
            .Include(x => x.Bairro)
            .Include(x => x.Regiao);

    public async Task AddAsync(Reporte reporte, CancellationToken cancellationToken)
    {
        await context.Reportes.AddAsync(reporte, cancellationToken);
    }
}
