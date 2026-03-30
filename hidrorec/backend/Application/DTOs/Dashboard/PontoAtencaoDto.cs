namespace HidroRec.Backend.Application.DTOs.Dashboard;

public sealed class PontoAtencaoDto
{
    public Guid Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Area { get; set; } = string.Empty;

    public string TipoOcorrencia { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Severidade { get; set; } = string.Empty;

    public DateTime DataOcorrencia { get; set; }
}
