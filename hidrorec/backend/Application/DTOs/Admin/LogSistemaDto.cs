namespace HidroRec.Backend.Application.DTOs.Admin;

public sealed class LogSistemaDto
{
    public Guid Id { get; set; }

    public string Nivel { get; set; } = string.Empty;

    public string Evento { get; set; } = string.Empty;

    public string Mensagem { get; set; } = string.Empty;

    public string Contexto { get; set; } = string.Empty;

    public DateTime DataCriacao { get; set; }
}
