using AutoMapper;
using HidroRec.Backend.Application.DTOs.Common;
using HidroRec.Backend.Application.DTOs.Reportes;
using HidroRec.Backend.Application.DTOs.Usuarios;
using HidroRec.Backend.Application.Interfaces;
using HidroRec.Backend.Domain.Entities;
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

    public async Task<UsuarioDto> UpdateOwnAsync(Guid id, UpdateOwnUsuarioRequestDto request, CancellationToken cancellationToken)
    {
        var usuario = await context.Usuarios
            .Include(x => x.Perfil)
            .FirstOrDefaultAsync(x => x.Id == id && !x.Excluido && x.Ativo, cancellationToken)
            ?? throw new KeyNotFoundException("Usuario nao encontrado.");

        usuario.Nome = request.Nome.Trim();
        usuario.Telefone = request.Telefone.Trim();
        usuario.DataAtualizacao = DateTime.UtcNow;

        await context.Auditorias.AddAsync(new Auditoria
        {
            Entidade = nameof(Usuario),
            EntidadeId = usuario.Id.ToString(),
            Acao = "AtualizacaoPerfilProprio",
            UsuarioId = usuario.Id,
            Detalhes = "Usuario atualizou seus dados de perfil."
        }, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return mapper.Map<UsuarioDto>(usuario);
    }

    public async Task<PagedResultDto<ReporteAdminItemDto>> GetOwnReportesAsync(Guid usuarioId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = context.Reportes
            .AsNoTracking()
            .Where(x => !x.Excluido && x.UsuarioId == usuarioId)
            .Include(x => x.Bairro)
            .Include(x => x.Regiao);

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.DataOcorrencia)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<ReporteAdminItemDto>
        {
            Items = mapper.Map<IReadOnlyCollection<ReporteAdminItemDto>>(items),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }
}
