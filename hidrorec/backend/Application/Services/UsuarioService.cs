using AutoMapper;
using HidroRec.Backend.Application.DTOs.Usuarios;
using HidroRec.Backend.Application.Interfaces;
using HidroRec.Backend.Domain.Interfaces;
using HidroRec.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HidroRec.Backend.Application.Services;

public sealed class UsuarioService(IUsuarioRepository usuarioRepository, HidroRecDbContext context, IMapper mapper) : IUsuarioService
{
    public async Task<IReadOnlyCollection<UsuarioDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var usuarios = await usuarioRepository.Query()
            .Where(x => !x.Excluido)
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);

        return mapper.Map<IReadOnlyCollection<UsuarioDto>>(usuarios);
    }

    public async Task<UsuarioDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.Query()
            .FirstOrDefaultAsync(x => x.Id == id && !x.Excluido, cancellationToken)
            ?? throw new KeyNotFoundException("Usuario nao encontrado.");

        return mapper.Map<UsuarioDto>(usuario);
    }

    public async Task<UsuarioDto> UpdateAsync(Guid id, UpdateUsuarioRequestDto request, CancellationToken cancellationToken)
    {
        var usuario = await context.Usuarios
            .Include(x => x.Perfil)
            .FirstOrDefaultAsync(x => x.Id == id && !x.Excluido, cancellationToken)
            ?? throw new KeyNotFoundException("Usuario nao encontrado.");

        var perfil = await context.Perfis.FirstOrDefaultAsync(x => x.Id == request.PerfilId, cancellationToken)
            ?? throw new KeyNotFoundException("Perfil nao encontrado.");

        usuario.Nome = request.Nome;
        usuario.Telefone = request.Telefone;
        usuario.Ativo = request.Ativo;
        usuario.PerfilId = perfil.Id;
        usuario.Perfil = perfil;
        usuario.DataAtualizacao = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return mapper.Map<UsuarioDto>(usuario);
    }
}
