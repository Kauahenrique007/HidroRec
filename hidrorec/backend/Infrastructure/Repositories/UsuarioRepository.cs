using HidroRec.Backend.Domain.Entities;
using HidroRec.Backend.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HidroRec.Backend.Infrastructure.Repositories;

public sealed class UsuarioRepository(HidroRec.Backend.Infrastructure.Data.HidroRecDbContext context) : IUsuarioRepository
{
    public IQueryable<Usuario> Query() =>
        context.Usuarios.Include(x => x.Perfil);

    public async Task AddAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        await context.Usuarios.AddAsync(usuario, cancellationToken);
    }
}
