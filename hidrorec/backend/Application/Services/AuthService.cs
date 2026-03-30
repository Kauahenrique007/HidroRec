using AutoMapper;
using HidroRec.Backend.Application.DTOs.Auth;
using HidroRec.Backend.Application.Interfaces;
using HidroRec.Backend.Domain.Entities;
using HidroRec.Backend.Domain.Interfaces;
using HidroRec.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HidroRec.Backend.Application.Services;

public sealed class AuthService(
    IUsuarioRepository usuarioRepository,
    HidroRecDbContext context,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IMapper mapper) : IAuthService
{
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken)
    {
        var exists = await usuarioRepository.Query().AnyAsync(x => x.Email == request.Email, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("Ja existe um usuario com este e-mail.");
        }

        var perfil = await context.Perfis.FirstAsync(x => x.Nome == "Cidadao", cancellationToken);
        var usuario = new Usuario
        {
            Nome = request.Nome,
            Email = request.Email.Trim().ToLowerInvariant(),
            SenhaHash = passwordHasher.Hash(request.Senha),
            Telefone = request.Telefone,
            PerfilId = perfil.Id,
            Perfil = perfil
        };

        await usuarioRepository.AddAsync(usuario, cancellationToken);
        await context.Auditorias.AddAsync(new Auditoria
        {
            Entidade = nameof(Usuario),
            EntidadeId = usuario.Id.ToString(),
            Acao = "Criacao",
            UsuarioId = usuario.Id,
            Detalhes = "Novo usuario colaborador cadastrado."
        }, cancellationToken);
        await context.LogsSistema.AddAsync(new LogSistema
        {
            Nivel = "Information",
            Evento = "AuthRegister",
            Mensagem = $"Usuario {usuario.Email} criado com sucesso.",
            Contexto = nameof(AuthService)
        }, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return BuildAuthResponse(usuario);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.Query()
            .FirstOrDefaultAsync(x => x.Email == request.Email.Trim().ToLowerInvariant() && x.Ativo, cancellationToken);

        if (usuario is null || !passwordHasher.Verify(request.Senha, usuario.SenhaHash))
        {
            throw new UnauthorizedAccessException("Credenciais invalidas.");
        }

        if (!usuario.SenhaHash.StartsWith("pbkdf2$", StringComparison.OrdinalIgnoreCase))
        {
            usuario.SenhaHash = passwordHasher.Hash(request.Senha);
        }

        usuario.DataAtualizacao = DateTime.UtcNow;
        await context.LogsSistema.AddAsync(new LogSistema
        {
            Nivel = "Information",
            Evento = "AuthLogin",
            Mensagem = $"Login realizado por {usuario.Email}.",
            Contexto = nameof(AuthService)
        }, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return BuildAuthResponse(usuario);
    }

    public async Task<UsuarioResumoDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.Query()
            .FirstOrDefaultAsync(x => x.Id == userId && x.Ativo && !x.Excluido, cancellationToken);

        return usuario is null
            ? throw new KeyNotFoundException("Sessao de usuario nao encontrada.")
            : mapper.Map<UsuarioResumoDto>(usuario);
    }

    private AuthResponseDto BuildAuthResponse(Usuario usuario)
    {
        var (token, expiraEm) = jwtTokenService.GenerateToken(usuario);
        return new AuthResponseDto
        {
            Token = token,
            ExpiraEm = expiraEm,
            Usuario = mapper.Map<UsuarioResumoDto>(usuario)
        };
    }
}
