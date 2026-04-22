namespace HidroRec.Backend.Application.DTOs.Usuarios;

public sealed class UpdateOwnUsuarioRequestDto
{
    public string Nome { get; set; } = string.Empty;

    public string Telefone { get; set; } = string.Empty;
}
