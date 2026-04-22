using HidroRec.Backend.Application.DTOs.Dashboard;
using HidroRec.Backend.Domain.Entities;

namespace HidroRec.Backend.Application.Interfaces;

public interface IRiskEngineService
{
    IReadOnlyCollection<AreaCriticaDto> AnalyzeAreas(
        IReadOnlyCollection<AreaMonitorada> areas,
        IReadOnlyCollection<Reporte> reportesRecentes,
        IReadOnlyCollection<Reporte> reportesHistoricos,
        IReadOnlyCollection<Alerta> alertasAtivos,
        decimal chuva24h,
        decimal mareAtual);

    IReadOnlyCollection<AtivoExpostoDto> AnalyzeAssets(
        IReadOnlyCollection<AtivoMonitorado> ativos,
        IReadOnlyCollection<AreaCriticaDto> areasCriticas);

    IReadOnlyCollection<DecisaoOperacionalDto> BuildOperationalDecisions(
        IReadOnlyCollection<AreaCriticaDto> areasCriticas,
        IReadOnlyCollection<AtivoExpostoDto> ativosExpostos);

    IReadOnlyCollection<FonteOperacionalDto> BuildSourceStatus(
        decimal chuva24h,
        string chuvaStatus,
        string chuvaDescricao,
        decimal mareAtual,
        string mareStatus,
        string mareDescricao,
        int reportesRecentes,
        int ativosMonitorados);
}
