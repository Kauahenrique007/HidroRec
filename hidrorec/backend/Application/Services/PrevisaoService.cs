using HidroRec.Backend.Application.DTOs.Previsoes;
using HidroRec.Backend.Application.Interfaces;

namespace HidroRec.Backend.Application.Services;

public sealed class PrevisaoService(IWeatherService weatherService, ITideService tideService) : IPrevisaoService
{
    public async Task<PrevisaoResumoDto> GetResumoAsync(CancellationToken cancellationToken)
    {
        var chuva = await weatherService.GetCurrentRainAsync(cancellationToken);
        var mare = await tideService.GetCurrentTideAsync(cancellationToken);
        var tendencia = ResolveRiskTrend(chuva.VolumeMmHora, mare.AlturaMetros);

        return new PrevisaoResumoDto
        {
            Indicadores =
            [
                new PrevisaoCardDto
                {
                    Titulo = "Chuva APAC (24h)",
                    Valor = $"{chuva.VolumeMmHora:0.0} mm",
                    Status = chuva.Status,
                    Descricao = chuva.Descricao
                },
                new PrevisaoCardDto
                {
                    Titulo = "Variacao de mare",
                    Valor = $"{mare.AlturaMetros:0.0} m",
                    Status = mare.Status,
                    Descricao = mare.Descricao
                },
                new PrevisaoCardDto
                {
                    Titulo = "Tendencia de risco",
                    Valor = tendencia.Valor,
                    Status = tendencia.Status,
                    Descricao = tendencia.Descricao
                }
            ],
            PeriodosCriticos =
            [
                new JanelaCriticaDto
                {
                    Periodo = "Agora",
                    NivelRisco = tendencia.NivelRisco,
                    Justificativa = $"Leitura oficial APAC de {chuva.VolumeMmHora:0.0} mm em 24h combinada com mare de {mare.AlturaMetros:0.0} m."
                },
                new JanelaCriticaDto
                {
                    Periodo = "Proximas horas",
                    NivelRisco = mare.AlturaMetros >= 2.3m || chuva.VolumeMmHora >= 30m ? "Atencao" : "Moderado",
                    Justificativa = "Manter acompanhamento operacional dos corredores com historico recorrente e drenagem sensivel."
                }
            ]
        };
    }

    public async Task<IReadOnlyCollection<PrevisaoCardDto>> GetChuvaAsync(CancellationToken cancellationToken)
    {
        var chuva = await weatherService.GetCurrentRainAsync(cancellationToken);
        return
        [
            new PrevisaoCardDto
            {
                Titulo = "Chuva APAC (24h)",
                Valor = $"{chuva.VolumeMmHora:0.0} mm",
                Status = chuva.Status,
                Descricao = chuva.Descricao
            }
        ];
    }

    public async Task<IReadOnlyCollection<PrevisaoCardDto>> GetMareAsync(CancellationToken cancellationToken)
    {
        var mare = await tideService.GetCurrentTideAsync(cancellationToken);
        return
        [
            new PrevisaoCardDto
            {
                Titulo = "Mare atual",
                Valor = $"{mare.AlturaMetros:0.0} m",
                Status = mare.Status,
                Descricao = mare.Descricao
            }
        ];
    }

    private static (string Valor, string Status, string Descricao, string NivelRisco) ResolveRiskTrend(decimal rain24h, decimal tideMeters)
    {
        if (rain24h >= 50m || (rain24h >= 30m && tideMeters >= 2.3m))
        {
            return ("Elevacao significativa", "Alto", "Persistencia de chuva acumulada forte com condicao maritima desfavoravel.", "Alto");
        }

        if (rain24h >= 20m || tideMeters >= 2.3m)
        {
            return ("Elevacao localizada", "Atencao", "Condicao de alerta para microalagamentos em corredores sensiveis.", "Atencao");
        }

        return ("Estavel", "Moderado", "Sem sinal externo de agravamento relevante nas leituras conectadas.", "Moderado");
    }
}
