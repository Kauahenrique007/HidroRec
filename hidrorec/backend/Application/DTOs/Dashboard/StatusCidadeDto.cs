namespace HidroRec.Backend.Application.DTOs.Dashboard;

public sealed class StatusCidadeDto
{
    public string NivelAtualRisco { get; set; } = string.Empty;

    public int Normal { get; set; }

    public int Atencao { get; set; }

    public int Alagamento { get; set; }

    public int AlagamentosAtivos { get; set; }
}
