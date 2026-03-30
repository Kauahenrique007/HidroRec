namespace HidroRec.Backend.Application.DTOs.Auth;

public sealed class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiraEm { get; set; }

    public UsuarioResumoDto Usuario { get; set; } = new();
}
