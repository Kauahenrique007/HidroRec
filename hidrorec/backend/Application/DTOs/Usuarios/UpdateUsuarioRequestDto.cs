namespace HidroRec.Backend.Application.DTOs.Usuarios;

public sealed class UpdateUsuarioRequestDto
{
    public string Nome { get; set; } = string.Empty;

    public string Telefone { get; set; } = string.Empty;

    public bool Ativo { get; set; }

    public int PerfilId { get; set; }
}
