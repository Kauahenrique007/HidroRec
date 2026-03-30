namespace HidroRec.Backend.Application.DTOs.Alertas;

public sealed class AlertaDto
{
    public Guid Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public string Criticidade { get; set; } = string.Empty;

    public bool Ativo { get; set; }

    public string AreaAfetada { get; set; } = string.Empty;

    public string OrientacaoResumida { get; set; } = string.Empty;

    public DateTime DataCriacao { get; set; }

    public IReadOnlyCollection<Guid> ReporteIds { get; set; } = Array.Empty<Guid>();
}
