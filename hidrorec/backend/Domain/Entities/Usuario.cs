namespace HidroRec.Backend.Domain.Entities;

public sealed class Usuario : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string SenhaHash { get; set; } = string.Empty;

    public string Telefone { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;

    public int PerfilId { get; set; }

    public Perfil Perfil { get; set; } = null!;

    public ICollection<Reporte> Reportes { get; set; } = new List<Reporte>();
}
