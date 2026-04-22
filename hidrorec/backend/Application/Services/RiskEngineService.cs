using HidroRec.Backend.Application.DTOs.Dashboard;
using HidroRec.Backend.Application.Interfaces;
using HidroRec.Backend.Domain.Entities;
using HidroRec.Backend.Domain.Enums;

namespace HidroRec.Backend.Application.Services;

public sealed class RiskEngineService : IRiskEngineService
{
    public IReadOnlyCollection<AreaCriticaDto> AnalyzeAreas(
        IReadOnlyCollection<AreaMonitorada> areas,
        IReadOnlyCollection<Reporte> reportesRecentes,
        IReadOnlyCollection<Reporte> reportesHistoricos,
        IReadOnlyCollection<Alerta> alertasAtivos,
        decimal chuva24h,
        decimal mareAtual)
    {
        var areasCriticas = areas
            .Where(area => area.Ativa)
            .Select(area =>
            {
                var recentesRelacionados = reportesRecentes
                    .Where(reporte => IsRelated(area, reporte))
                    .ToArray();

                var historicosRelacionados = reportesHistoricos
                    .Where(reporte => IsRelated(area, reporte))
                    .ToArray();

                var alertasRelacionados = alertasAtivos
                    .Count(alerta => area.BairroId.HasValue && alerta.BairroId == area.BairroId);

                var ativosSensveis = area.AtivosMonitorados.Count(x => x.Ativo && x.Sensibilidade >= 75);
                var ativosExpostos = area.AtivosMonitorados.Count(x => x.Ativo);
                var reportesSeveros = recentesRelacionados.Count(x => x.Severidade is SeveridadeReporte.Alagamento or SeveridadeReporte.AlagamentoCritico);
                var reportesAtencao = recentesRelacionados.Count(x => x.Severidade == SeveridadeReporte.Atencao);
                var reportesAtivos = recentesRelacionados.Count(x => x.Status is not StatusReporte.Resolvido and not StatusReporte.Arquivado);
                var recorrencia = historicosRelacionados.Count(x => x.DataOcorrencia >= DateTime.UtcNow.AddDays(-30));

                var score = CalculateScore(
                    area.Vulnerabilidade,
                    area.CriticidadeOperacional,
                    chuva24h,
                    mareAtual,
                    reportesAtivos,
                    reportesSeveros,
                    reportesAtencao,
                    recorrencia,
                    alertasRelacionados,
                    ativosSensveis,
                    ativosExpostos);

                var nivel = ResolveRiskLevel(score);
                var janela = ResolveCriticalWindow(score, chuva24h, mareAtual);
                var tendencia = ResolveTrend(chuva24h, mareAtual, reportesSeveros, alertasRelacionados, reportesAtivos);
                var prioridade = ResolvePriority(score);

                return new AreaCriticaDto
                {
                    Id = area.Id,
                    Nome = area.Nome,
                    Organizacao = area.Organizacao.Nome,
                    Bairro = area.Bairro?.Nome ?? string.Empty,
                    Regiao = area.Regiao?.Nome ?? string.Empty,
                    Latitude = area.Latitude,
                    Longitude = area.Longitude,
                    ScoreRisco = score,
                    NivelRisco = nivel,
                    Tendencia = tendencia,
                    Prioridade = prioridade,
                    ReportesRecentes = reportesAtivos,
                    AtivosExpostos = ativosExpostos,
                    JanelaCritica = janela,
                    AcaoSugerida = ResolveAreaAction(score, ativosSensveis, reportesSeveros, area.Tipo),
                    LeituraExplicavel = BuildExplanation(chuva24h, mareAtual, area, reportesAtivos, recorrencia, ativosSensveis, alertasRelacionados)
                };
            })
            .OrderByDescending(area => area.ScoreRisco)
            .ThenByDescending(area => area.AtivosExpostos)
            .Take(6)
            .ToArray();

        return areasCriticas;
    }

    public IReadOnlyCollection<AtivoExpostoDto> AnalyzeAssets(
        IReadOnlyCollection<AtivoMonitorado> ativos,
        IReadOnlyCollection<AreaCriticaDto> areasCriticas)
    {
        var riscosPorArea = areasCriticas.ToDictionary(area => area.Id);

        return ativos
            .Where(ativo => ativo.Ativo && ativo.AreaMonitoradaId.HasValue && riscosPorArea.ContainsKey(ativo.AreaMonitoradaId.Value))
            .Select(ativo =>
            {
                var area = riscosPorArea[ativo.AreaMonitoradaId!.Value];
                var score = Math.Min(100, area.ScoreRisco + ResolveAssetAdjustment(ativo.Sensibilidade, ativo.StatusOperacional));

                return new AtivoExpostoDto
                {
                    Id = ativo.Id,
                    Nome = ativo.Nome,
                    Tipo = ativo.Tipo,
                    Organizacao = ativo.Organizacao.Nome,
                    Area = area.Nome,
                    ScoreRisco = score,
                    NivelRisco = ResolveRiskLevel(score),
                    StatusOperacional = ativo.StatusOperacional,
                    AcaoSugerida = ResolveAssetAction(score, ativo.Tipo, area.Nome)
                };
            })
            .OrderByDescending(ativo => ativo.ScoreRisco)
            .ThenByDescending(ativo => ativo.StatusOperacional)
            .Take(5)
            .ToArray();
    }

    public IReadOnlyCollection<DecisaoOperacionalDto> BuildOperationalDecisions(
        IReadOnlyCollection<AreaCriticaDto> areasCriticas,
        IReadOnlyCollection<AtivoExpostoDto> ativosExpostos)
    {
        var decisoes = new List<DecisaoOperacionalDto>();

        var areaPrincipal = areasCriticas.FirstOrDefault();
        if (areaPrincipal is not null)
        {
            decisoes.Add(new DecisaoOperacionalDto
            {
                Titulo = areaPrincipal.ScoreRisco >= 75 ? "Priorizar resposta imediata" : "Reforcar monitoramento territorial",
                Prioridade = areaPrincipal.Prioridade,
                Area = areaPrincipal.Nome,
                JanelaCritica = areaPrincipal.JanelaCritica,
                Acao = areaPrincipal.AcaoSugerida,
                Fundamentacao = areaPrincipal.LeituraExplicavel
            });
        }

        var ativoPrincipal = ativosExpostos.FirstOrDefault();
        if (ativoPrincipal is not null)
        {
            decisoes.Add(new DecisaoOperacionalDto
            {
                Titulo = "Proteger ativo mais exposto",
                Prioridade = ResolvePriority(ativoPrincipal.ScoreRisco),
                Area = ativoPrincipal.Area,
                JanelaCritica = ativoPrincipal.ScoreRisco >= 75 ? "Agora" : "Proximas horas",
                Acao = ativoPrincipal.AcaoSugerida,
                Fundamentacao = $"{ativoPrincipal.Nome} em {ativoPrincipal.Area} com score {ativoPrincipal.ScoreRisco}."
            });
        }

        foreach (var area in areasCriticas.Skip(1).Take(2))
        {
            decisoes.Add(new DecisaoOperacionalDto
            {
                Titulo = "Ajustar prontidao de campo",
                Prioridade = area.Prioridade,
                Area = area.Nome,
                JanelaCritica = area.JanelaCritica,
                Acao = area.AcaoSugerida,
                Fundamentacao = area.LeituraExplicavel
            });
        }

        return decisoes
            .DistinctBy(x => $"{x.Titulo}|{x.Area}")
            .Take(4)
            .ToArray();
    }

    public IReadOnlyCollection<FonteOperacionalDto> BuildSourceStatus(
        decimal chuva24h,
        string chuvaStatus,
        string chuvaDescricao,
        decimal mareAtual,
        string mareStatus,
        string mareDescricao,
        int reportesRecentes,
        int ativosMonitorados)
    {
        var agora = DateTime.UtcNow;

        return
        [
            new FonteOperacionalDto
            {
                Nome = "APAC",
                Status = chuvaStatus,
                Detalhe = $"{chuva24h:0.0} mm/24h | {chuvaDescricao}",
                AtualizadoEm = agora
            },
            new FonteOperacionalDto
            {
                Nome = "Mare operacional",
                Status = mareStatus,
                Detalhe = $"{mareAtual:0.0} m | {mareDescricao}",
                AtualizadoEm = agora
            },
            new FonteOperacionalDto
            {
                Nome = "Rede colaborativa",
                Status = reportesRecentes > 0 ? "Ativa" : "Estavel",
                Detalhe = $"{reportesRecentes} ocorrencias recentes consolidadas",
                AtualizadoEm = agora
            },
            new FonteOperacionalDto
            {
                Nome = "Ativos monitorados",
                Status = ativosMonitorados > 0 ? "Cobertura ativa" : "Sem cobertura",
                Detalhe = $"{ativosMonitorados} ativos operacionais vinculados",
                AtualizadoEm = agora
            }
        ];
    }

    private static bool IsRelated(AreaMonitorada area, Reporte reporte)
    {
        if (reporte.AreaMonitoradaId.HasValue && reporte.AreaMonitoradaId == area.Id)
        {
            return true;
        }

        if (area.BairroId.HasValue && reporte.BairroId.HasValue && area.BairroId == reporte.BairroId)
        {
            return true;
        }

        return false;
    }

    private static int CalculateScore(
        int vulnerabilidade,
        int criticidadeOperacional,
        decimal chuva24h,
        decimal mareAtual,
        int reportesAtivos,
        int reportesSeveros,
        int reportesAtencao,
        int recorrencia,
        int alertasRelacionados,
        int ativosSensveis,
        int ativosExpostos)
    {
        var vulnerabilityComponent = (int)Math.Round(vulnerabilidade * 0.18m, MidpointRounding.AwayFromZero);
        var criticalityComponent = (int)Math.Round(criticidadeOperacional * 0.16m, MidpointRounding.AwayFromZero);
        var rainComponent = chuva24h switch
        {
            >= 60m => 20,
            >= 40m => 16,
            >= 25m => 11,
            >= 12m => 6,
            _ => 2
        };
        var tideComponent = mareAtual switch
        {
            >= 2.5m => 12,
            >= 2.3m => 8,
            >= 2.1m => 5,
            _ => 2
        };
        var recentPressure = Math.Min(16, reportesAtivos * 4);
        var severityPressure = Math.Min(18, (reportesSeveros * 7) + (reportesAtencao * 3));
        var recurrencePressure = Math.Min(10, recorrencia * 2);
        var alertPressure = Math.Min(10, alertasRelacionados * 5);
        var assetPressure = Math.Min(10, (ativosSensveis * 4) + Math.Min(3, ativosExpostos));

        return Math.Min(100,
            vulnerabilityComponent +
            criticalityComponent +
            rainComponent +
            tideComponent +
            recentPressure +
            severityPressure +
            recurrencePressure +
            alertPressure +
            assetPressure);
    }

    private static string ResolveRiskLevel(int score) => score switch
    {
        >= 75 => "Critico",
        >= 50 => "Alto",
        >= 25 => "Moderado",
        _ => "Baixo"
    };

    private static string ResolvePriority(int score) => score switch
    {
        >= 75 => "Imediata",
        >= 50 => "Alta",
        >= 25 => "Direcionada",
        _ => "Monitoramento"
    };

    private static string ResolveTrend(decimal chuva24h, decimal mareAtual, int reportesSeveros, int alertasRelacionados, int reportesAtivos)
    {
        if (chuva24h >= 30m || mareAtual >= 2.3m || reportesSeveros > 0 || alertasRelacionados > 0)
        {
            return "Agravando";
        }

        if (reportesAtivos > 0)
        {
            return "Instavel";
        }

        return "Estavel";
    }

    private static string ResolveCriticalWindow(int score, decimal chuva24h, decimal mareAtual)
    {
        if (score >= 75 || (chuva24h >= 30m && mareAtual >= 2.3m))
        {
            return "Agora e proximas 2h";
        }

        if (score >= 50 || chuva24h >= 20m)
        {
            return "Proximas 4h";
        }

        return "Observacao nas proximas 6h";
    }

    private static string ResolveAreaAction(int score, int ativosSensveis, int reportesSeveros, string tipoArea)
    {
        if (score >= 75 && ativosSensveis > 0)
        {
            return "Ativar protocolo preventivo e proteger unidades expostas";
        }

        if (score >= 75 || reportesSeveros > 0)
        {
            return "Redirecionar equipe de campo e validar bloqueios locais";
        }

        if (score >= 50)
        {
            return $"Intensificar monitoramento em {tipoArea.ToLowerInvariant()} e revisar drenagem sensivel";
        }

        if (score >= 25)
        {
            return "Manter observacao direcionada e checagem preventiva";
        }

        return "Acompanhar sem acionamento adicional";
    }

    private static string BuildExplanation(
        decimal chuva24h,
        decimal mareAtual,
        AreaMonitorada area,
        int reportesAtivos,
        int recorrencia,
        int ativosSensveis,
        int alertasRelacionados)
    {
        return $"{chuva24h:0.0} mm/24h, mare {mareAtual:0.0} m, vulnerabilidade {area.Vulnerabilidade}, criticidade {area.CriticidadeOperacional}, {reportesAtivos} ocorrencias recentes, recorrencia {recorrencia}, {ativosSensveis} ativos sensiveis, {alertasRelacionados} alertas ativos.";
    }

    private static int ResolveAssetAdjustment(int sensibilidade, string statusOperacional)
    {
        var sensitivity = sensibilidade switch
        {
            >= 85 => 14,
            >= 70 => 10,
            >= 55 => 6,
            _ => 3
        };

        var status = statusOperacional.Contains("restr", StringComparison.OrdinalIgnoreCase)
            ? 6
            : statusOperacional.Contains("conting", StringComparison.OrdinalIgnoreCase)
                ? 4
                : 2;

        return sensitivity + status;
    }

    private static string ResolveAssetAction(int score, string tipo, string area)
    {
        if (score >= 75)
        {
            return $"Proteger {tipo.ToLowerInvariant()} em {area} e revisar continuidade operacional";
        }

        if (score >= 50)
        {
            return $"Preparar contingencia para {tipo.ToLowerInvariant()} em {area}";
        }

        return $"Manter acompanhamento de {tipo.ToLowerInvariant()} em {area}";
    }
}
