namespace HidroRec.Backend.Application.DTOs.Admin;

public sealed class AuditoriaDto
{
    public Guid Id { get; set; }

    public string Entidade { get; set; } = string.Empty;

    public string EntidadeId { get; set; } = string.Empty;

    public string Acao { get; set; } = string.Empty;

    public string Usuario { get; set; } = string.Empty;

    public string Detalhes { get; set; } = string.Empty;

    public DateTime DataCriacao { get; set; }
}
