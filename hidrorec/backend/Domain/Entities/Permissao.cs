namespace HidroRec.Backend.Domain.Entities;

public sealed class Permissao : BaseEntity
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Codigo { get; set; } = string.Empty;

    public ICollection<PerfilPermissao> PerfilPermissoes { get; set; } = new List<PerfilPermissao>();
}
