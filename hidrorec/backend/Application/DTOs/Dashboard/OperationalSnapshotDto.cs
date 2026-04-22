namespace HidroRec.Backend.Application.DTOs.Dashboard;

public sealed class OperationalSnapshotDto
{
    public IReadOnlyCollection<AreaCriticaDto> AreasCriticas { get; set; } = Array.Empty<AreaCriticaDto>();

    public IReadOnlyCollection<AtivoExpostoDto> AtivosExpostos { get; set; } = Array.Empty<AtivoExpostoDto>();

    public IReadOnlyCollection<DecisaoOperacionalDto> DecisoesOperacionais { get; set; } = Array.Empty<DecisaoOperacionalDto>();

    public IReadOnlyCollection<FonteOperacionalDto> Fontes { get; set; } = Array.Empty<FonteOperacionalDto>();
}
