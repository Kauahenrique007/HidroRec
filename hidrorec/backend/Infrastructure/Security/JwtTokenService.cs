using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HidroRec.Backend.Application.Interfaces;
using HidroRec.Backend.Configurations;
using HidroRec.Backend.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HidroRec.Backend.Infrastructure.Security;

public sealed class JwtTokenService(Microsoft.Extensions.Options.IOptions<JwtOptions> jwtOptions) : IJwtTokenService
{
    public (string Token, DateTime ExpiraEm) GenerateToken(Usuario usuario)
    {
        var options = jwtOptions.Value;
        var expires = DateTime.UtcNow.AddMinutes(options.ExpiresInMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Role, usuario.Perfil.Nome),
            new("perfil", usuario.Perfil.Nome)
        };

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var serialized = new JwtSecurityTokenHandler().WriteToken(token);
        return (serialized, expires);
    }
}
