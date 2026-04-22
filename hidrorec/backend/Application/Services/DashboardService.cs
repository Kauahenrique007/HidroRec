using HidroRec.Backend.Application.DTOs.DataFusion;
using HidroRec.Backend.Application.DTOs.Dashboard;
using HidroRec.Backend.Application.Interfaces;
using HidroRec.Backend.Domain.Entities;
using HidroRec.Backend.Domain.Enums;
using HidroRec.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HidroRec.Backend.Application.Services;

public sealed class DashboardService(
    HidroRecDbContext context,
    IDataFusionService dataFusionService,
    IRiskEngineService riskEngineService) : IDashboardService
{
    public async Task<DashboardResumoDto> GetResumoAsync(CancellationToken cancellationToken)
    {
        var agora = DateTime.UtcNow;
        var counts = await BaseQuery()
            .GroupBy(_ => 1)
            .Select(group => new DashboardCounts
            {
                Normal = group.Count(x => x.Severidade == SeveridadeReporte.Normal),
                Atencao = group.Count(x => x.Severidade == SeveridadeReporte.Atencao),
                Alagamento = group.Count(x => x.Severidade == SeveridadeReporte.Alagamento || x.Severidade == SeveridadeReporte.AlagamentoCritico),
                AlagamentosAtivos = group.Count(x => x.Status != StatusReporte.Resolvido &&
                                                     x.Status != StatusReporte.Arquivado &&
                                                     (x.Severidade == SeveridadeReporte.Alagamento || x.Severidade == SeveridadeReporte.AlagamentoCritico))
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? new DashboardCounts();

        var reportes = await GetRecentProjectionQuery()
            .Take(14)
            .ToListAsync(cancellationToken);

        var reportesRecentes = await BaseQuery()
            .Include(x => x.Bairro)
            .Include(x => x.Regiao)
            .Include(x => x.AreaMonitorada)
            .Where(x => x.DataOcorrencia >= agora.AddHours(-8))
            .ToListAsync(cancellationToken);

        var reportesHistoricos = await BaseQuery()
            .Include(x => x.Bairro)
            .Include(x => x.Regiao)
            .Include(x => x.AreaMonitorada)
            .Where(x => x.DataOcorrencia >= agora.AddDays(-30))
            .ToListAsync(cancellationToken);

        var areas = await context.AreasMonitoradas
            .AsNoTracking()
            .Include(x => x.Organizacao)
            .Include(x => x.Bairro)
            .Include(x => x.Regiao)
            .Include(x => x.AtivosMonitorados.Where(ativo => ativo.Ativo))
            .Where(x => x.Ativa && !x.Excluido)
            .ToListAsync(cancellationToken);

        var ativos = await context.AtivosMonitorados
            .AsNoTracking()
            .Include(x => x.Organizacao)
            .Include(x => x.AreaMonitorada)
            .Where(x => x.Ativo && !x.Excluido)
            .ToListAsync(cancellationToken);

        var alertasAtivos = await context.Alertas
            .AsNoTracking()
            .Where(x => x.Ativo && !x.Excluido)
            .ToListAsync(cancellationToken);

        var snapshot = await dataFusionService.GetSnapshotAsync(null, null, null, cancellationToken);
        var indicadores = BuildIndicators(snapshot);

        var areasCriticas = riskEngineService.AnalyzeAreas(areas, reportesRecentes, reportesHistoricos, alertasAtivos, snapshot.Climate.RainObserved24hMm, snapshot.Tide.CurrentLevelMeters);
        var ativosExpostos = riskEngineService.AnalyzeAssets(ativos, areasCriticas);
        var decisoes = riskEngineService.BuildOperationalDecisions(areasCriticas, ativosExpostos);
        var fontes = BuildSourceCards(snapshot.Sources);

        var areaPrincipal = areasCriticas.FirstOrDefault();
        var statusCidade = ResolveCityRisk(Math.Max(areaPrincipal?.ScoreRisco ?? 0, snapshot.Risk.Score), counts);
        var situacaoAtual = BuildOperationalSituation(snapshot, areaPrincipal, counts.AlagamentosAtivos);

        return new DashboardResumoDto
        {
            StatusCidade = new StatusCidadeDto
            {
                NivelAtualRisco = statusCidade,
                SituacaoAtual = situacaoAtual.SituacaoAtual,
                LeituraOficial = situacaoAtual.LeituraOficial,
                EstagioOperacional = situacaoAtual.EstagioOperacional,
                AtualizadoEm = snapshot.GeneratedAtUtc,
                Normal = counts.Normal,
                Atencao = counts.Atencao,
                Alagamento = counts.Alagamento,
                AlagamentosAtivos = counts.AlagamentosAtivos
            },
            Indicadores = indicadores,
            Governanca = BuildGovernance(areasCriticas, fontes, decisoes, snapshot),
            Mapa = BuildMap(reportes, areasCriticas).Take(8).ToArray(),
            PontosAtencao = BuildAttention(reportes).Take(5).ToArray(),
            AreasCriticas = areasCriticas.Take(4).ToArray(),
            AtivosExpostos = ativosExpostos.Take(4).ToArray(),
            DecisoesOperacionais = decisoes.Take(4).ToArray(),
            Fontes = fontes,
            BannerAlerta = BuildBanner(areaPrincipal, counts.AlagamentosAtivos)
        };
    }

    public async Task<IReadOnlyCollection<IndicadorCardDto>> GetIndicadoresAsync(CancellationToken cancellationToken)
    {
        var resumo = await GetResumoAsync(cancellationToken);
        return [resumo.Indicadores.MareAtual, resumo.Indicadores.VolumeChuva];
    }

    public async Task<IReadOnlyCollection<PontoAtencaoDto>> GetPontosRecentesAsync(CancellationToken cancellationToken)
    {
        var resumo = await GetResumoAsync(cancellationToken);
        return resumo.PontosAtencao;
    }

    public async Task<IReadOnlyCollection<MapaPontoDto>> GetMapaAsync(CancellationToken cancellationToken)
    {
        var resumo = await GetResumoAsync(cancellationToken);
        return resumo.Mapa;
    }

    public async Task<IReadOnlyCollection<PontoAtencaoDto>> GetPontosAtencaoAsync(CancellationToken cancellationToken)
    {
        var resumo = await GetResumoAsync(cancellationToken);
        return resumo.PontosAtencao;
    }

    private IQueryable<Reporte> BaseQuery() =>
        context.Reportes
            .AsNoTracking()
            .Where(x => !x.Excluido);

    private IQueryable<DashboardProjection> GetRecentProjectionQuery() =>
        context.Reportes
            .AsNoTracking()
            .Where(x => !x.Excluido)
            .OrderByDescending(x => x.DataOcorrencia)
            .Select(x => new DashboardProjection
            {
                Id = x.Id,
                Titulo = x.Titulo,
                EnderecoReferencia = x.EnderecoReferencia,
                BairroNome = x.Bairro != null ? x.Bairro.Nome : x.BairroNome,
                RegiaoNome = x.Regiao != null ? x.Regiao.Nome : x.RegiaoNome,
                AreaMonitoradaId = x.AreaMonitoradaId,
                Status = x.Status,
                Severidade = x.Severidade,
                TipoOcorrencia = x.TipoOcorrencia,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                DataOcorrencia = x.DataOcorrencia
            });

    private static IndicadoresResumoDto BuildIndicators(OperationalDataFusionSnapshotDto snapshot)
    {
        return new IndicadoresResumoDto
        {
            MareAtual = new IndicadorCardDto
            {
                Titulo = "Mare Atual",
                ValorPrincipal = $"{snapshot.Tide.Trend} - {snapshot.Tide.CurrentLevelMeters:0.0}m",
                Complemento = $"{snapshot.Tide.Summary} Confiabilidade {snapshot.Tide.Reliability:P0}."
            },
            VolumeChuva = new IndicadorCardDto
            {
                Titulo = "Chuva Acumulada",
                ValorPrincipal = $"{snapshot.Climate.RainObserved24hMm:0.0} mm/24h - {snapshot.Risk.Level}",
                Complemento = $"{snapshot.Climate.Summary} Prox. 6h: {snapshot.Climate.Forecast6hMm:0.0} mm | prox. 24h: {snapshot.Climate.Forecast24hMm:0.0} mm | pico {snapshot.Climate.PeakHourlyRainMm:0.0} mm/h."
            }
        };
    }

    private static IReadOnlyCollection<FonteOperacionalDto> BuildSourceCards(IReadOnlyCollection<DataSourceStatusDto> sources)
    {
        return sources
            .OrderByDescending(item => item.IsFresh)
            .ThenByDescending(item => item.Reliability)
            .Select(item => new FonteOperacionalDto
            {
                Nome = item.Source,
                Status = item.Status,
                Detalhe = $"{item.Category} | {item.FreshnessLabel.ToLowerInvariant()} | conf. {item.Reliability:P0} | {(item.FallbackUsed ? "fallback ativo | " : string.Empty)}{item.Summary}",
                AtualizadoEm = item.TimestampUtc
            })
            .ToArray();
    }

    private static IEnumerable<MapaPontoDto> BuildMap(
        IEnumerable<DashboardProjection> reportes,
        IReadOnlyCollection<AreaCriticaDto> areasCriticas)
    {
        var areasById = areasCriticas.ToDictionary(area => area.Id);
        var areasByBairro = areasCriticas
            .Where(area => !string.IsNullOrWhiteSpace(area.Bairro))
            .GroupBy(area => area.Bairro, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(item => item.ScoreRisco).First(), StringComparer.OrdinalIgnoreCase);

        return reportes.Select(reporte =>
        {
            AreaCriticaDto? area = null;

            if (reporte.AreaMonitoradaId.HasValue && areasById.TryGetValue(reporte.AreaMonitoradaId.Value, out var byId))
            {
                area = byId;
            }
            else if (!string.IsNullOrWhiteSpace(reporte.BairroNome) && areasByBairro.TryGetValue(reporte.BairroNome, out var byBairro))
            {
                area = byBairro;
            }

            return new MapaPontoDto
            {
                Id = reporte.Id,
                Titulo = area?.Nome ?? reporte.Titulo,
                Bairro = !string.IsNullOrWhiteSpace(reporte.BairroNome) ? reporte.BairroNome : reporte.RegiaoNome,
                Status = reporte.Status.ToString(),
                Severidade = area is null ? reporte.Severidade.ToString() : area.NivelRisco,
                Latitude = reporte.Latitude,
                Longitude = reporte.Longitude,
                ScoreRisco = area?.ScoreRisco ?? ResolveFallbackScore(reporte.Severidade),
                Categoria = area?.Organizacao ?? reporte.TipoOcorrencia.ToString()
            };
        });
    }

    private static IEnumerable<PontoAtencaoDto> BuildAttention(IEnumerable<DashboardProjection> reportes) =>
        reportes
            .OrderByDescending(x => x.Status is not StatusReporte.Resolvido and not StatusReporte.Arquivado)
            .ThenByDescending(x => x.DataOcorrencia)
            .ThenByDescending(x => x.Severidade)
            .ThenByDescending(x => x.Status == StatusReporte.Confirmado)
            .Select(reporte => new PontoAtencaoDto
            {
                Id = reporte.Id,
                Titulo = reporte.Titulo,
                Area = $"{reporte.EnderecoReferencia}, {reporte.BairroNome}",
                TipoOcorrencia = reporte.TipoOcorrencia.ToString(),
                Status = reporte.Status.ToString(),
                Severidade = reporte.Severidade.ToString(),
                DataOcorrencia = reporte.DataOcorrencia
            });

    private static string ResolveCityRisk(int scorePrincipal, DashboardCounts counts)
    {
        if (scorePrincipal >= 75 || counts.AlagamentosAtivos >= 2)
        {
            return "Risco Critico";
        }

        if (scorePrincipal >= 50 || counts.Alagamento > 0)
        {
            return "Risco Alto";
        }

        if (scorePrincipal >= 25 || counts.Atencao > 0)
        {
            return "Risco Moderado";
        }

        return "Risco Baixo";
    }

    private static string BuildBanner(AreaCriticaDto? areaPrincipal, int alagamentosAtivos)
    {
        if (areaPrincipal is null)
        {
            return "Sem zonas criticas priorizadas no momento.";
        }

        if (areaPrincipal.ScoreRisco >= 75)
        {
            return $"{areaPrincipal.Nome} em prioridade imediata | score {areaPrincipal.ScoreRisco} | {alagamentosAtivos} alagamentos ativos.";
        }

        if (areaPrincipal.ScoreRisco >= 50)
        {
            return $"{areaPrincipal.Nome} lidera a criticidade territorial | score {areaPrincipal.ScoreRisco}.";
        }

        return "Sem agravamento territorial relevante nas ultimas leituras.";
    }

    private static (string SituacaoAtual, string LeituraOficial, string EstagioOperacional) BuildOperationalSituation(
        OperationalDataFusionSnapshotDto snapshot,
        AreaCriticaDto? areaPrincipal,
        int alagamentosAtivos)
    {
        var leituraOficial = $"{snapshot.Climate.RainObserved24hMm:0.0} mm/24h | prox. 6h {snapshot.Climate.Forecast6hMm:0.0} mm | prox. 24h {snapshot.Climate.Forecast24hMm:0.0} mm | mare {snapshot.Tide.CurrentLevelMeters:0.0} m | risco {snapshot.Risk.Score}/100";

        if (areaPrincipal is not null && areaPrincipal.ScoreRisco >= 75)
        {
            return (
                $"Maior pressao territorial em {areaPrincipal.Nome}",
                leituraOficial,
                "Resposta imediata e protecao de ativos");
        }

        if (snapshot.Risk.Score >= 50 || snapshot.Climate.Forecast6hMm >= 4m || snapshot.Climate.Forecast24hMm >= 10m || snapshot.Climate.PeakHourlyRainMm >= 2m || snapshot.Tide.CurrentLevelMeters >= 2.3m || alagamentosAtivos > 0)
        {
            return (
                $"Janela de atencao com pico previsto por volta de {snapshot.Climate.PeakHourlyAtUtc.ToLocalTime():HH\\h}",
                leituraOficial,
                "Monitoramento reforcado, triagem prioritaria e checagem antes do pico previsto");
        }

        return (
            "Leitura urbana estavel nas fontes conectadas",
            leituraOficial,
            "Monitoramento preventivo");
    }

    private static GovernancaOperacionalDto BuildGovernance(
        IReadOnlyCollection<AreaCriticaDto> areasCriticas,
        IReadOnlyCollection<FonteOperacionalDto> fontes,
        IReadOnlyCollection<DecisaoOperacionalDto> decisoes,
        OperationalDataFusionSnapshotDto snapshot)
    {
        var bairros = areasCriticas
            .Select(x => x.Bairro)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        return new GovernancaOperacionalDto
        {
            AtualizadoEm = snapshot.GeneratedAtUtc,
            ConfiabilidadeLeitura = snapshot.Risk.Reliability >= 0.85m ? "Alta" : snapshot.Risk.Reliability >= 0.70m ? "Moderada" : "Degradada",
            CoberturaTerritorial = bairros > 0 ? $"{bairros} bairros e {areasCriticas.Count} zonas priorizadas" : "Cobertura territorial em consolidacao",
            FontesAtivas = string.Join(" | ", fontes.Select(x => x.Nome)),
            DiretrizExecutiva = decisoes.FirstOrDefault()?.Acao ?? "Manter monitoramento operacional"
        };
    }

    private static int ResolveFallbackScore(SeveridadeReporte severidade) => severidade switch
    {
        SeveridadeReporte.AlagamentoCritico => 85,
        SeveridadeReporte.Alagamento => 68,
        SeveridadeReporte.Atencao => 42,
        _ => 18
    };

    private sealed class DashboardProjection
    {
        public Guid Id { get; init; }
        public string Titulo { get; init; } = string.Empty;
        public string EnderecoReferencia { get; init; } = string.Empty;
        public string BairroNome { get; init; } = string.Empty;
        public string RegiaoNome { get; init; } = string.Empty;
        public int? AreaMonitoradaId { get; init; }
        public StatusReporte Status { get; init; }
        public SeveridadeReporte Severidade { get; init; }
        public TipoOcorrencia TipoOcorrencia { get; init; }
        public decimal Latitude { get; init; }
        public decimal Longitude { get; init; }
        public DateTime DataOcorrencia { get; init; }
    }

    private sealed class DashboardCounts
    {
        public int Normal { get; init; }
        public int Atencao { get; init; }
        public int Alagamento { get; init; }
        public int AlagamentosAtivos { get; init; }
    }
}
