namespace HidroRec.Backend.Application.DTOs.Dashboard;

public sealed class StatusCidadeDto
{
    public string NivelAtualRisco { get; set; } = string.Empty;

    public string SituacaoAtual { get; set; } = string.Empty;

    public string LeituraOficial { get; set; } = string.Empty;

    public string EstagioOperacional { get; set; } = string.Empty;

    public DateTime AtualizadoEm { get; set; }

    public int Normal { get; set; }

    public int Atencao { get; set; }

    public int Alagamento { get; set; }

    public int AlagamentosAtivos { get; set; }
}
