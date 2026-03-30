using HidroRec.Backend.Domain.Entities;

namespace HidroRec.Backend.Application.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiraEm) GenerateToken(Usuario usuario);
}
