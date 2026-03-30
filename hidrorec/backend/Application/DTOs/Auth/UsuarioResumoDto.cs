namespace HidroRec.Backend.Application.DTOs.Auth;

public sealed class UsuarioResumoDto
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Perfil { get; set; } = string.Empty;
}
