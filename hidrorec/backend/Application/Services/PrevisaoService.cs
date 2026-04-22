using HidroRec.Backend.Application.DTOs.DataFusion;
using HidroRec.Backend.Application.DTOs.Previsoes;
using HidroRec.Backend.Application.Interfaces;

namespace HidroRec.Backend.Application.Services;

public sealed class PrevisaoService(IDataFusionService dataFusionService) : IPrevisaoService
{
    public async Task<PrevisaoResumoDto> GetResumoAsync(CancellationToken cancellationToken)
    {
        var snapshot = await dataFusionService.GetSnapshotAsync(null, null, null, cancellationToken);
        var tendencia = ResolveRiskTrend(snapshot);
        var forecastSource = snapshot.Sources.FirstOrDefault(item => item.Category == "Previsao horaria");
        var leituraOficial = $"{snapshot.Climate.RainObserved24hMm:0.0} mm/24h observados | prox. 6h {snapshot.Climate.Forecast6hMm:0.0} mm | prox. 24h {snapshot.Climate.Forecast24hMm:0.0} mm | mare {snapshot.Tide.CurrentLevelMeters:0.0} m";

        return new PrevisaoResumoDto
        {
            SituacaoAtual = ResolveCurrentSituation(snapshot, tendencia.NivelRisco),
            LeituraOficial = leituraOficial,
            Recomendacao = ResolveRecommendation(snapshot, tendencia.NivelRisco),
            AtualizadoEm = snapshot.GeneratedAtUtc,
            Janela24h = $"Nas proximas 24h a leitura fusionada projeta {snapshot.Climate.Forecast24hMm:0.0} mm, com pico de {snapshot.Climate.PeakHourlyRainMm:0.0} mm/h por volta de {snapshot.Climate.PeakHourlyAtUtc.ToLocalTime():HH\\h}.",
            Indicadores =
            [
                new PrevisaoCardDto
                {
                    Titulo = "Chuva APAC (24h)",
                    Valor = $"{snapshot.Climate.RainObserved24hMm:0.0} mm",
                    Status = snapshot.Risk.Level,
                    Descricao = $"{snapshot.Climate.Summary} Confiabilidade {snapshot.Climate.Reliability:P0}."
                },
                new PrevisaoCardDto
                {
                    Titulo = "Proximas 6h",
                    Valor = $"{snapshot.Climate.Forecast6hMm:0.0} mm",
                    Status = tendencia.Status,
                    Descricao = $"Probabilidade maxima {snapshot.Climate.MaxProbabilityPercent}% | fonte horaria confiabilidade {(forecastSource?.Reliability ?? snapshot.Climate.Reliability):P0}."
                },
                new PrevisaoCardDto
                {
                    Titulo = "Proximas 24h",
                    Valor = $"{snapshot.Climate.Forecast24hMm:0.0} mm",
                    Status = ResolveAccumulated24hStatus(snapshot.Climate.Forecast24hMm),
                    Descricao = "Acumulado projetado para as proximas 24h com dados horarios consolidados."
                },
                new PrevisaoCardDto
                {
                    Titulo = "Pico horario previsto",
                    Valor = $"{snapshot.Climate.PeakHourlyRainMm:0.0} mm/h",
                    Status = $"{snapshot.Climate.PeakHourlyAtUtc.ToLocalTime():HH\\h}",
                    Descricao = $"Probabilidade maxima {snapshot.Climate.MaxProbabilityPercent}% | {tendencia.Descricao}"
                },
                new PrevisaoCardDto
                {
                    Titulo = "Variacao de mare",
                    Valor = $"{snapshot.Tide.CurrentLevelMeters:0.0} m",
                    Status = snapshot.Tide.Trend,
                    Descricao = $"{snapshot.Tide.Summary} Confiabilidade {snapshot.Tide.Reliability:P0}."
                }
            ],
            PeriodosCriticos =
            [
                new JanelaCriticaDto
                {
                    Periodo = "Agora",
                    NivelRisco = tendencia.NivelRisco,
                    Justificativa = $"Leitura oficial de {leituraOficial} | score operacional {snapshot.Risk.Score}/100."
                },
                new JanelaCriticaDto
                {
                    Periodo = "0-6h",
                    NivelRisco = ResolveForecastWindowRisk(snapshot.Climate.Forecast6hMm, snapshot.Climate.PeakHourlyRainMm, snapshot.Climate.MaxProbabilityPercent, snapshot.Tide.CurrentLevelMeters),
                    Justificativa = $"Acumulado previsto de {snapshot.Climate.Forecast6hMm:0.0} mm nas proximas 6h. Pico previsto {snapshot.Climate.PeakHourlyRainMm:0.0} mm/h por volta de {snapshot.Climate.PeakHourlyAtUtc.ToLocalTime():HH\\h}."
                },
                new JanelaCriticaDto
                {
                    Periodo = "6-12h",
                    NivelRisco = ResolveAccumulatedWindowRisk(snapshot.Climate.Forecast12hMm - snapshot.Climate.Forecast6hMm),
                    Justificativa = $"Acumulado adicional de {(snapshot.Climate.Forecast12hMm - snapshot.Climate.Forecast6hMm):0.0} mm entre 6h e 12h."
                },
                new JanelaCriticaDto
                {
                    Periodo = "12-24h",
                    NivelRisco = ResolveAccumulatedWindowRisk(snapshot.Climate.Forecast24hMm - snapshot.Climate.Forecast12hMm),
                    Justificativa = $"Acumulado adicional de {(snapshot.Climate.Forecast24hMm - snapshot.Climate.Forecast12hMm):0.0} mm entre 12h e 24h."
                }
            ]
        };
    }

    public async Task<IReadOnlyCollection<PrevisaoCardDto>> GetChuvaAsync(CancellationToken cancellationToken)
    {
        var snapshot = await dataFusionService.GetSnapshotAsync(null, null, null, cancellationToken);
        return
        [
            new PrevisaoCardDto
            {
                Titulo = "Chuva APAC (24h)",
                Valor = $"{snapshot.Climate.RainObserved24hMm:0.0} mm",
                Status = snapshot.Risk.Level,
                Descricao = snapshot.Climate.Summary
            }
        ];
    }

    public async Task<IReadOnlyCollection<PrevisaoCardDto>> GetMareAsync(CancellationToken cancellationToken)
    {
        var snapshot = await dataFusionService.GetSnapshotAsync(null, null, null, cancellationToken);
        return
        [
            new PrevisaoCardDto
            {
                Titulo = "Mare atual",
                Valor = $"{snapshot.Tide.CurrentLevelMeters:0.0} m",
                Status = snapshot.Tide.Trend,
                Descricao = snapshot.Tide.Summary
            }
        ];
    }

    private static (string Valor, string Status, string Descricao, string NivelRisco) ResolveRiskTrend(OperationalDataFusionSnapshotDto snapshot)
    {
        if (snapshot.Risk.Score >= 75 || snapshot.Climate.Forecast24hMm >= 20m || snapshot.Climate.Forecast6hMm >= 8m || snapshot.Climate.PeakHourlyRainMm >= 4m || (snapshot.Climate.RainObserved24hMm >= 30m && snapshot.Tide.CurrentLevelMeters >= 2.3m))
        {
            return ("Elevacao significativa", "Alto", "Persistencia de chuva acumulada forte com intensificacao horaria prevista.", "Alto");
        }

        if (snapshot.Risk.Score >= 50 || snapshot.Climate.Forecast6hMm >= 4m || snapshot.Climate.Forecast24hMm >= 10m || snapshot.Climate.MaxProbabilityPercent >= 70 || snapshot.Tide.CurrentLevelMeters >= 2.3m)
        {
            return ("Elevacao localizada", "Atencao", "Condicao de alerta para microalagamentos com agravamento localizado nas proximas horas.", "Atencao");
        }

        return ("Estavel", "Moderado", "Sem sinal externo de agravamento relevante nas leituras conectadas.", "Moderado");
    }

    private static string ResolveCurrentSituation(OperationalDataFusionSnapshotDto snapshot, string nivelRisco)
    {
        if (snapshot.Climate.Forecast6hMm >= 6m || snapshot.Climate.Forecast24hMm >= 12m || snapshot.Climate.PeakHourlyRainMm >= 2m || snapshot.Climate.RainObserved24hMm >= 30m)
        {
            return "Recife opera com intensificacao prevista nas proximas horas";
        }

        if (snapshot.Tide.CurrentLevelMeters >= 2.3m || snapshot.Climate.MaxProbabilityPercent >= 70 || nivelRisco.Equals("Atencao", StringComparison.OrdinalIgnoreCase))
        {
            return "Recife segue com pressao localizada em corredores sensiveis";
        }

        return "Recife sem agravamento relevante nas leituras conectadas";
    }

    private static string ResolveRecommendation(OperationalDataFusionSnapshotDto snapshot, string nivelRisco)
    {
        if (snapshot.Climate.RainObserved24hMm >= 30m || snapshot.Climate.Forecast6hMm >= 6m || snapshot.Climate.Forecast24hMm >= 12m || snapshot.Climate.PeakHourlyRainMm >= 2m || nivelRisco.Equals("Alto", StringComparison.OrdinalIgnoreCase))
        {
            return "Reforcar monitoramento territorial, validar corredores historicos e preparar contingencia antes do pico previsto.";
        }

        if (snapshot.Tide.CurrentLevelMeters >= 2.3m || snapshot.Climate.MaxProbabilityPercent >= 70 || nivelRisco.Equals("Atencao", StringComparison.OrdinalIgnoreCase))
        {
            return "Acompanhar corredores sensiveis, atualizar equipes e manter triagem operacional ativa.";
        }

        return "Manter observacao preventiva e atualizacao rotineira das fontes.";
    }

    private static string ResolveForecastWindowRisk(decimal accumulatedWindow, decimal peakHourly, int probabilityMax, decimal tideMeters)
    {
        if (accumulatedWindow >= 6m || peakHourly >= 2m)
        {
            return "Atencao";
        }

        if (tideMeters >= 2.3m || probabilityMax >= 60)
        {
            return "Moderado";
        }

        return "Baixo";
    }

    private static string ResolveAccumulatedWindowRisk(decimal accumulatedWindow)
    {
        if (accumulatedWindow >= 8m) return "Atencao";
        if (accumulatedWindow >= 4m) return "Moderado";
        return "Baixo";
    }

    private static string ResolveAccumulated24hStatus(decimal accumulated24h)
    {
        if (accumulated24h >= 20m) return "Elevada";
        if (accumulated24h >= 10m) return "Atencao";
        if (accumulated24h >= 4m) return "Monitorada";
        return "Baixa";
    }
}
