using AutoMapper;
using HidroRec.Backend.Application.DTOs.Alertas;
using HidroRec.Backend.Application.Interfaces;
using HidroRec.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HidroRec.Backend.Application.Services;

public sealed class AlertaService(HidroRecDbContext context, IMapper mapper) : IAlertaService
{
    public async Task<IReadOnlyCollection<AlertaDto>> GetAllAsync(bool somenteAtivos, string? criticidade, CancellationToken cancellationToken)
    {
        var query = context.Alertas
            .AsNoTracking()
            .Include(x => x.AlertaReportes)
            .Where(x => !x.Excluido);

        if (somenteAtivos)
        {
            query = query.Where(x => x.Ativo);
        }

        if (!string.IsNullOrWhiteSpace(criticidade))
        {
            query = query.Where(x => x.Criticidade.ToString() == criticidade);
        }

        var items = await query.OrderByDescending(x => x.DataCriacao).ToListAsync(cancellationToken);
        return mapper.Map<IReadOnlyCollection<AlertaDto>>(items);
    }

    public async Task<AlertaDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var alerta = await context.Alertas
            .AsNoTracking()
            .Include(x => x.AlertaReportes)
            .FirstOrDefaultAsync(x => x.Id == id && !x.Excluido, cancellationToken)
            ?? throw new KeyNotFoundException("Alerta nao encontrado.");

        return mapper.Map<AlertaDto>(alerta);
    }
}
