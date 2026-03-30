namespace HidroRec.Backend.Domain.Entities;

public sealed class Auditoria : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Entidade { get; set; } = string.Empty;

    public string EntidadeId { get; set; } = string.Empty;

    public string Acao { get; set; } = string.Empty;

    public string Detalhes { get; set; } = string.Empty;

    public Guid? UsuarioId { get; set; }

    public Usuario? Usuario { get; set; }
}
