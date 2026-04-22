using AutoMapper;
using HidroRec.Backend.Application.DTOs.Common;
using HidroRec.Backend.Application.DTOs.Reportes;
using HidroRec.Backend.Application.Interfaces;
using HidroRec.Backend.Configurations;
using HidroRec.Backend.Domain.Entities;
using HidroRec.Backend.Domain.Enums;
using HidroRec.Backend.Domain.Interfaces;
using HidroRec.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HidroRec.Backend.Application.Services;

public sealed class ReporteService(
    HidroRecDbContext context,
    IReporteRepository reporteRepository,
    IMapper mapper,
    IOptions<FileStorageOptions> storageOptions,
    ILogger<ReporteService> logger) : IReporteService
{
    public async Task<ReporteDto> CreateAsync(CreateReporteRequestDto request, Guid? usuarioId, CancellationToken cancellationToken)
    {
        var bairro = await context.Bairros.FirstOrDefaultAsync(x => x.Nome == request.Bairro, cancellationToken);
        var regiao = await context.Regioes.FirstOrDefaultAsync(x => x.Nome == request.Regiao, cancellationToken);
        var areaMonitoradaId = await ResolveAreaMonitoradaIdAsync(
            bairro?.Id,
            regiao?.Id,
            request.Bairro,
            request.Latitude,
            request.Longitude,
            cancellationToken);

        var reporte = new Reporte
        {
            Titulo = request.Titulo,
            Descricao = request.Descricao,
            NivelAgua = request.NivelAgua,
            TipoOcorrencia = request.TipoOcorrencia,
            Severidade = ResolveSeverity(request.NivelAgua),
            Status = StatusReporte.Pendente,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            EnderecoReferencia = request.EnderecoReferencia,
            BairroId = bairro?.Id,
            Bairro = bairro,
            RegiaoId = regiao?.Id,
            Regiao = regiao,
            AreaMonitoradaId = areaMonitoradaId,
            BairroNome = request.Bairro,
            RegiaoNome = request.Regiao,
            UsuarioId = usuarioId,
            NomeUsuario = request.NomeUsuario,
            ContatoUsuario = request.ContatoUsuario,
            CaminhoImagem = await SaveImageAsync(request, cancellationToken),
            Observacoes = request.Observacoes,
            Fonte = request.Fonte,
            DataOcorrencia = request.DataOcorrencia ?? DateTime.UtcNow
        };

        await reporteRepository.AddAsync(reporte, cancellationToken);
        await context.HistoricosReporte.AddAsync(new HistoricoReporte
        {
            ReporteId = reporte.Id,
            StatusAnterior = StatusReporte.Pendente,
            StatusNovo = StatusReporte.Pendente,
            Observacao = "Reporte criado no fluxo colaborativo.",
            AlteradoPorUsuarioId = usuarioId
        }, cancellationToken);
        await context.Auditorias.AddAsync(new Auditoria
        {
            Entidade = nameof(Reporte),
            EntidadeId = reporte.Id.ToString(),
            Acao = "Criacao",
            UsuarioId = usuarioId,
            Detalhes = $"Reporte criado com severidade {reporte.Severidade}."
        }, cancellationToken);
        await context.LogsSistema.AddAsync(new LogSistema
        {
            Nivel = "Information",
            Evento = "ReporteCriado",
            Mensagem = $"Reporte {reporte.Titulo} registrado.",
            Contexto = nameof(ReporteService)
        }, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Reporte {ReporteId} criado com sucesso", reporte.Id);

        return mapper.Map<ReporteDto>(reporte);
    }

    public async Task<PagedResultDto<ReporteAdminItemDto>> GetAllAsync(int page, int pageSize, string? status, string? bairro, CancellationToken cancellationToken)
    {
        var query = reporteRepository.Query().Where(x => !x.Excluido);

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

    public async Task<ReporteDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var reporte = await context.Reportes
            .AsNoTracking()
            .Include(x => x.Bairro)
            .Include(x => x.Regiao)
            .Include(x => x.Historicos)
            .ThenInclude(x => x.AlteradoPorUsuario)
            .FirstOrDefaultAsync(x => x.Id == id && !x.Excluido, cancellationToken);

        return reporte is null
            ? throw new KeyNotFoundException("Reporte nao encontrado.")
            : mapper.Map<ReporteDto>(reporte);
    }

    public async Task<ReporteDto> UpdateAsync(Guid id, UpdateReporteRequestDto request, CancellationToken cancellationToken)
    {
        var reporte = await context.Reportes
            .Include(x => x.Bairro)
            .Include(x => x.Regiao)
            .Include(x => x.Historicos)
            .FirstOrDefaultAsync(x => x.Id == id && !x.Excluido, cancellationToken)
            ?? throw new KeyNotFoundException("Reporte nao encontrado.");

        reporte.Titulo = request.Titulo;
        reporte.Descricao = request.Descricao;
        reporte.TipoOcorrencia = request.TipoOcorrencia;
        reporte.EnderecoReferencia = request.EnderecoReferencia;
        reporte.Observacoes = request.Observacoes;
        reporte.DataAtualizacao = DateTime.UtcNow;
        reporte.AreaMonitoradaId = await ResolveAreaMonitoradaIdAsync(
            reporte.BairroId,
            reporte.RegiaoId,
            reporte.BairroNome,
            reporte.Latitude,
            reporte.Longitude,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return mapper.Map<ReporteDto>(reporte);
    }

    public async Task<ReporteDto> UpdateStatusAsync(Guid id, UpdateReporteStatusRequestDto request, Guid? usuarioId, CancellationToken cancellationToken)
    {
        var reporte = await context.Reportes
            .Include(x => x.AlertaReportes)
            .ThenInclude(x => x.Alerta)
            .Include(x => x.Bairro)
            .Include(x => x.Regiao)
            .Include(x => x.Historicos)
            .ThenInclude(x => x.AlteradoPorUsuario)
            .FirstOrDefaultAsync(x => x.Id == id && !x.Excluido, cancellationToken)
            ?? throw new KeyNotFoundException("Reporte nao encontrado.");

        var previousStatus = reporte.Status;
        reporte.Status = request.Status;
        reporte.DataAtualizacao = DateTime.UtcNow;

        await context.HistoricosReporte.AddAsync(new HistoricoReporte
        {
            ReporteId = reporte.Id,
            StatusAnterior = previousStatus,
            StatusNovo = request.Status,
            Observacao = request.Observacao,
            AlteradoPorUsuarioId = usuarioId
        }, cancellationToken);

        await EnsureAlertConsistencyAsync(reporte, cancellationToken);

        await context.Auditorias.AddAsync(new Auditoria
        {
            Entidade = nameof(Reporte),
            EntidadeId = reporte.Id.ToString(),
            Acao = "AtualizacaoStatus",
            UsuarioId = usuarioId,
            Detalhes = $"Status alterado de {previousStatus} para {request.Status}."
        }, cancellationToken);
        await context.LogsSistema.AddAsync(new LogSistema
        {
            Nivel = "Information",
            Evento = "ReporteStatusAtualizado",
            Mensagem = $"Reporte {reporte.Id} atualizado para {request.Status}.",
            Contexto = nameof(ReporteService)
        }, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return mapper.Map<ReporteDto>(reporte);
    }

    public async Task DeleteAsync(Guid id, Guid? usuarioId, CancellationToken cancellationToken)
    {
        var reporte = await context.Reportes.FirstOrDefaultAsync(x => x.Id == id && !x.Excluido, cancellationToken)
            ?? throw new KeyNotFoundException("Reporte nao encontrado.");

        reporte.Excluido = true;
        reporte.DataAtualizacao = DateTime.UtcNow;

        await context.Auditorias.AddAsync(new Auditoria
        {
            Entidade = nameof(Reporte),
            EntidadeId = reporte.Id.ToString(),
            Acao = "ExclusaoLogica",
            UsuarioId = usuarioId,
            Detalhes = "Reporte removido logicamente."
        }, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureAlertConsistencyAsync(Reporte reporte, CancellationToken cancellationToken)
    {
        var shouldHaveAlert = reporte.Status != StatusReporte.Resolvido &&
                              (reporte.Severidade == SeveridadeReporte.Alagamento || reporte.Severidade == SeveridadeReporte.AlagamentoCritico);
        var currentAlert = reporte.AlertaReportes.FirstOrDefault()?.Alerta;

        if (shouldHaveAlert && currentAlert is null)
        {
            var alerta = new Alerta
            {
                Titulo = $"Alerta: {reporte.Titulo}",
                Descricao = reporte.Descricao,
                Criticidade = reporte.Severidade == SeveridadeReporte.AlagamentoCritico ? CriticidadeAlerta.Critico : CriticidadeAlerta.Alagamento,
                AreaAfetada = $"{reporte.EnderecoReferencia}, {reporte.BairroNome}",
                OrientacaoResumida = "Evite o trecho e acompanhe as atualizacoes operacionais.",
                BairroId = reporte.BairroId,
                Ativo = true
            };

            await context.Alertas.AddAsync(alerta, cancellationToken);
            await context.AlertasReportes.AddAsync(new AlertaReporte
            {
                AlertaId = alerta.Id,
                ReporteId = reporte.Id
            }, cancellationToken);
        }

        if (!shouldHaveAlert && currentAlert is not null)
        {
            currentAlert.Ativo = false;
            currentAlert.DataAtualizacao = DateTime.UtcNow;
        }
    }

    private async Task<string?> SaveImageAsync(CreateReporteRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ImagemBase64) || string.IsNullOrWhiteSpace(request.ImagemNomeArquivo))
        {
            return null;
        }

        var options = storageOptions.Value;
        var uploadsPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, options.UploadsPath));
        Directory.CreateDirectory(uploadsPath);

        var fileExtension = Path.GetExtension(request.ImagemNomeArquivo);
        var fileName = $"{Guid.NewGuid():N}{fileExtension}";
        var fullPath = Path.Combine(uploadsPath, fileName);
        var bytes = Convert.FromBase64String(request.ImagemBase64);

        await File.WriteAllBytesAsync(fullPath, bytes, cancellationToken);
        return $"/uploads/{fileName}";
    }

    private static SeveridadeReporte ResolveSeverity(NivelAgua nivelAgua) =>
        nivelAgua switch
        {
            NivelAgua.Pocas => SeveridadeReporte.Normal,
            NivelAgua.Tornozelo => SeveridadeReporte.Atencao,
            NivelAgua.Joelho => SeveridadeReporte.Atencao,
            NivelAgua.Cintura => SeveridadeReporte.Alagamento,
            NivelAgua.Peito => SeveridadeReporte.AlagamentoCritico,
            _ => SeveridadeReporte.Atencao
        };

    private async Task<int?> ResolveAreaMonitoradaIdAsync(
        int? bairroId,
        int? regiaoId,
        string? bairroNome,
        decimal latitude,
        decimal longitude,
        CancellationToken cancellationToken)
    {
        var query = context.AreasMonitoradas
            .AsNoTracking()
            .Include(x => x.Bairro)
            .Where(x => x.Ativa && !x.Excluido);

        if (bairroId.HasValue)
        {
            var sameNeighborhoodAreas = await query
                .Where(x => x.BairroId == bairroId)
                .ToListAsync(cancellationToken);

            var area = PickNearestArea(sameNeighborhoodAreas, latitude, longitude);
            if (area is not null)
            {
                return area.Id;
            }
        }

        if (!string.IsNullOrWhiteSpace(bairroNome))
        {
            var normalizedNeighborhood = bairroNome.Trim();
            var sameNeighborhoodByName = await query
                .Where(x => x.Bairro != null && x.Bairro.Nome == normalizedNeighborhood)
                .ToListAsync(cancellationToken);

            var area = PickNearestArea(sameNeighborhoodByName, latitude, longitude);
            if (area is not null)
            {
                return area.Id;
            }
        }

        if (regiaoId.HasValue)
        {
            var sameRegionAreas = await query
                .Where(x => x.RegiaoId == regiaoId)
                .ToListAsync(cancellationToken);

            var nearbyArea = PickNearestArea(sameRegionAreas, latitude, longitude, maxDistanceKm: 3.5d);
            if (nearbyArea is not null)
            {
                return nearbyArea.Id;
            }
        }

        return null;
    }

    private static AreaMonitorada? PickNearestArea(
        IReadOnlyCollection<AreaMonitorada> areas,
        decimal latitude,
        decimal longitude,
        double? maxDistanceKm = null)
    {
        if (areas.Count == 0)
        {
            return null;
        }

        var rankedAreas = areas
            .Select(area => new
            {
                Area = area,
                DistanceKm = CalculateDistanceKm(latitude, longitude, area.Latitude, area.Longitude)
            })
            .OrderBy(x => x.DistanceKm)
            .ThenByDescending(x => x.Area.CriticidadeOperacional)
            .ToArray();

        var nearest = rankedAreas.FirstOrDefault();
        if (nearest is null)
        {
            return null;
        }

        if (maxDistanceKm.HasValue && nearest.DistanceKm > maxDistanceKm.Value)
        {
            return null;
        }

        return nearest.Area;
    }

    private static double CalculateDistanceKm(decimal latitudeA, decimal longitudeA, decimal latitudeB, decimal longitudeB)
    {
        const double earthRadiusKm = 6371d;
        var lat1 = DegreesToRadians((double)latitudeA);
        var lon1 = DegreesToRadians((double)longitudeA);
        var lat2 = DegreesToRadians((double)latitudeB);
        var lon2 = DegreesToRadians((double)longitudeB);

        var deltaLat = lat2 - lat1;
        var deltaLon = lon2 - lon1;

        var sinLat = Math.Sin(deltaLat / 2d);
        var sinLon = Math.Sin(deltaLon / 2d);

        var a = (sinLat * sinLat) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                (sinLon * sinLon);

        var c = 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));
        return earthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * (Math.PI / 180d);
}
