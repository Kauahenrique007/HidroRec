using HidroRec.Backend.Domain.Entities;
using HidroRec.Backend.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HidroRec.Backend.Infrastructure.Repositories;

public sealed class ReporteRepository(HidroRec.Backend.Infrastructure.Data.HidroRecDbContext context) : IReporteRepository
{
    public IQueryable<Reporte> Query() =>
        context.Reportes
            .Include(x => x.Bairro)
            .Include(x => x.Regiao)
            .Include(x => x.Historicos)
            .ThenInclude(x => x.AlteradoPorUsuario)
            .Include(x => x.AlertaReportes);

    public async Task AddAsync(Reporte reporte, CancellationToken cancellationToken)
    {
        await context.Reportes.AddAsync(reporte, cancellationToken);
    }
}
