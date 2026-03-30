namespace HidroRec.Backend.Application.DTOs.Dashboard;

public sealed class IndicadoresResumoDto
{
    public IndicadorCardDto MareAtual { get; set; } = new();

    public IndicadorCardDto VolumeChuva { get; set; } = new();
}
