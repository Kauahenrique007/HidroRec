namespace HidroRec.Backend.Domain.Entities;

public sealed class LogSistema : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Nivel { get; set; } = string.Empty;

    public string Evento { get; set; } = string.Empty;

    public string Mensagem { get; set; } = string.Empty;

    public string Contexto { get; set; } = string.Empty;
}
