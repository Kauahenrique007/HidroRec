using HidroRec.Backend.Application.DTOs.Dashboard;
using HidroRec.Backend.Application.Interfaces;
using HidroRec.Backend.Domain.Enums;
using HidroRec.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HidroRec.Backend.Application.Services;

public sealed class DashboardService(
    HidroRecDbContext context,
    IWeatherService weatherService,
    ITideService tideService) : IDashboardService
{
    public async Task<DashboardResumoDto> GetResumoAsync(CancellationToken cancellationToken)
    {
        var reportes = await BaseQuery().ToListAsync(cancellationToken);
        var indicators = await BuildIndicatorsAsync(cancellationToken);
        var activeFloods = reportes.Count(x => x.Status != StatusReporte.Resolvido &&
                                              (x.Severidade == SeveridadeReporte.Alagamento || x.Severidade == SeveridadeReporte.AlagamentoCritico));

        return new DashboardResumoDto
        {
            StatusCidade = new StatusCidadeDto
            {
                NivelAtualRisco = ResolveCityRisk(reportes),
                Normal = reportes.Count(x => x.Severidade == SeveridadeReporte.Normal),
                Atencao = reportes.Count(x => x.Severidade == SeveridadeReporte.Atencao),
                Alagamento = reportes.Count(x => x.Severidade == SeveridadeReporte.Alagamento || x.Severidade == SeveridadeReporte.AlagamentoCritico),
                AlagamentosAtivos = activeFloods
            },
            Indicadores = indicators,
            Mapa = BuildMap(reportes).Take(6).ToArray(),
            PontosAtencao = BuildAttention(reportes).Take(4).ToArray(),
            BannerAlerta = activeFloods > 0
                ? $"{activeFloods} alagamentos ativos detectados. Evite as areas afetadas."
                : "Sem alagamentos ativos criticos no momento."
        };
    }

    public async Task<IReadOnlyCollection<IndicadorCardDto>> GetIndicadoresAsync(CancellationToken cancellationToken)
    {
        var indicators = await BuildIndicatorsAsync(cancellationToken);
        return [indicators.MareAtual, indicators.VolumeChuva];
    }

    public async Task<IReadOnlyCollection<PontoAtencaoDto>> GetPontosRecentesAsync(CancellationToken cancellationToken)
    {
        var reportes = await BaseQuery().OrderByDescending(x => x.DataOcorrencia).Take(8).ToListAsync(cancellationToken);
        return BuildAttention(reportes).ToArray();
    }

    public async Task<IReadOnlyCollection<MapaPontoDto>> GetMapaAsync(CancellationToken cancellationToken)
    {
        var reportes = await BaseQuery().OrderByDescending(x => x.DataOcorrencia).Take(8).ToListAsync(cancellationToken);
        return BuildMap(reportes).ToArray();
    }

    public async Task<IReadOnlyCollection<PontoAtencaoDto>> GetPontosAtencaoAsync(CancellationToken cancellationToken)
    {
        var reportes = await BaseQuery().OrderByDescending(x => x.DataOcorrencia).Take(4).ToListAsync(cancellationToken);
        return BuildAttention(reportes).ToArray();
    }

    private IQueryable<Domain.Entities.Reporte> BaseQuery() =>
        context.Reportes
            .AsNoTracking()
            .Include(x => x.Bairro)
            .Where(x => !x.Excluido);

    private async Task<IndicadoresResumoDto> BuildIndicatorsAsync(CancellationToken cancellationToken)
    {
        var rain = await weatherService.GetCurrentRainAsync(cancellationToken);
        var tide = await tideService.GetCurrentTideAsync(cancellationToken);

        return new IndicadoresResumoDto
        {
            MareAtual = new IndicadorCardDto
            {
                Titulo = "Mare Atual",
                ValorPrincipal = $"{tide.Status} - {tide.AlturaMetros:0.0}m",
                Complemento = tide.Descricao
            },
            VolumeChuva = new IndicadorCardDto
            {
                Titulo = "Chuva Acumulada",
                ValorPrincipal = $"{rain.VolumeMmHora:0.0} mm/24h - {rain.Status}",
                Complemento = rain.Descricao
            }
        };
    }

    private static IEnumerable<MapaPontoDto> BuildMap(IEnumerable<Domain.Entities.Reporte> reportes) =>
        reportes.Select(reporte => new MapaPontoDto
        {
            Id = reporte.Id,
            Titulo = reporte.Titulo,
            Bairro = reporte.Bairro?.Nome ?? reporte.BairroNome,
            Status = reporte.Status.ToString(),
            Severidade = reporte.Severidade.ToString(),
            Latitude = reporte.Latitude,
            Longitude = reporte.Longitude
        });

    private static IEnumerable<PontoAtencaoDto> BuildAttention(IEnumerable<Domain.Entities.Reporte> reportes) =>
        reportes
            .OrderByDescending(x => x.Severidade)
            .ThenByDescending(x => x.DataOcorrencia)
            .Select(reporte => new PontoAtencaoDto
            {
                Id = reporte.Id,
                Titulo = reporte.Titulo,
                Area = $"{reporte.EnderecoReferencia}, {reporte.Bairro?.Nome ?? reporte.BairroNome}",
                TipoOcorrencia = reporte.TipoOcorrencia.ToString(),
                Status = reporte.Status.ToString(),
                Severidade = reporte.Severidade.ToString(),
                DataOcorrencia = reporte.DataOcorrencia
            });

    private static string ResolveCityRisk(IEnumerable<Domain.Entities.Reporte> reportes)
    {
        if (reportes.Any(x => x.Severidade == SeveridadeReporte.Alagamento || x.Severidade == SeveridadeReporte.AlagamentoCritico))
        {
            return "Risco Alto";
        }

        if (reportes.Any(x => x.Severidade == SeveridadeReporte.Atencao))
        {
            return "Risco Moderado";
        }

        return "Risco Baixo";
    }
}
