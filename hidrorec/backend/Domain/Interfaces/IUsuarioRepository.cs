using HidroRec.Backend.Domain.Entities;

namespace HidroRec.Backend.Domain.Interfaces;

public interface IUsuarioRepository
{
    IQueryable<Usuario> Query();

    Task AddAsync(Usuario usuario, CancellationToken cancellationToken);
}
