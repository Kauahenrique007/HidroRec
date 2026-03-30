namespace HidroRec.Backend.Domain.Entities;

public sealed class PerfilPermissao
{
    public int PerfilId { get; set; }

    public Perfil Perfil { get; set; } = null!;

    public int PermissaoId { get; set; }

    public Permissao Permissao { get; set; } = null!;
}
