using AutoMapper;
using HidroRec.Backend.Application.DTOs.Admin;
using HidroRec.Backend.Application.DTOs.Common;
using HidroRec.Backend.Application.DTOs.Reportes;
using HidroRec.Backend.Application.Interfaces;
using HidroRec.Backend.Domain.Enums;
using HidroRec.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HidroRec.Backend.Application.Services;

public sealed class AdminService(HidroRecDbContext context, IMapper mapper) : IAdminService
{
    public async Task<PagedResultDto<ReporteAdminItemDto>> GetReportesAsync(int page, int pageSize, string? status, string? bairro, CancellationToken cancellationToken)
    {
        var query = context.Reportes
            .AsNoTracking()
            .Include(x => x.Bairro)
            .Include(x => x.Regiao)
            .Where(x => !x.Excluido);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<StatusReporte>(status, true, out var parsedStatus))
        {
            query = query.Where(x => x.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(bairro))
        {
            query = query.Where(x => (x.Bairro != null && x.Bairro.Nome.Contains(bairro)) || x.BairroNome.Contains(bairro));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.DataOcorrencia)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<ReporteAdminItemDto>
        {
            Items = mapper.Map<IReadOnlyCollection<ReporteAdminItemDto>>(items),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<AdminMetricasDto> GetMetricasAsync(CancellationToken cancellationToken)
    {
        var since24h = DateTime.UtcNow.AddHours(-24);
        return new AdminMetricasDto
        {
            TotalReportes = await context.Reportes.CountAsync(x => !x.Excluido, cancellationToken),
            Pendentes = await context.Reportes.CountAsync(x => x.Status == StatusReporte.Pendente && !x.Excluido, cancellationToken),
            Confirmados = await context.Reportes.CountAsync(x => x.Status == StatusReporte.Confirmado && !x.Excluido, cancellationToken),
            AlertasAtivos = await context.Alertas.CountAsync(x => x.Ativo && !x.Excluido, cancellationToken),
            UsuariosAtivos = await context.Usuarios.CountAsync(x => x.Ativo && !x.Excluido, cancellationToken),
            OrganizacoesAtivas = await context.Organizacoes.CountAsync(x => x.Ativa && !x.Excluido, cancellationToken),
            AreasMonitoradas = await context.AreasMonitoradas.CountAsync(x => x.Ativa && !x.Excluido, cancellationToken),
            AtivosMonitorados = await context.AtivosMonitorados.CountAsync(x => x.Ativo && !x.Excluido, cancellationToken),
            LeiturasFusionCriticas24h = await context.LogsSistema.CountAsync(
                x => x.DataCriacao >= since24h &&
                     EF.Functions.Like(x.Contexto, "data-fusion%") &&
                     (x.Evento == "data-fusion-alert" || x.Evento == "data-fusion-risk"),
                cancellationToken),
            FontesDegradadas24h = await context.LogsSistema.CountAsync(
                x => x.DataCriacao >= since24h &&
                     EF.Functions.Like(x.Contexto, "data-fusion%") &&
                     (x.Contexto.Contains("Fallback") || x.Contexto.Contains("Desatualizado")),
                cancellationToken)
        };
    }

    public async Task<IReadOnlyCollection<AuditoriaDto>> GetAuditoriaAsync(CancellationToken cancellationToken)
    {
        var items = await context.Auditorias
            .AsNoTracking()
            .Include(x => x.Usuario)
            .OrderByDescending(x => x.DataCriacao)
            .Take(50)
            .ToListAsync(cancellationToken);

        return mapper.Map<IReadOnlyCollection<AuditoriaDto>>(items);
    }

    public async Task<IReadOnlyCollection<LogSistemaDto>> GetLogsAsync(string? contexto, string? evento, CancellationToken cancellationToken)
    {
        var query = context.LogsSistema
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(contexto))
        {
            query = query.Where(x => x.Contexto.Contains(contexto));
        }

        if (!string.IsNullOrWhiteSpace(evento))
        {
            query = query.Where(x => x.Evento == evento);
        }

        var items = await query
            .OrderByDescending(x => x.DataCriacao)
            .Take(50)
            .ToListAsync(cancellationToken);

        return mapper.Map<IReadOnlyCollection<LogSistemaDto>>(items);
    }
}
