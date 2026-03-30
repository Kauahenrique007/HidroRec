using HidroRec.Backend.Application.Interfaces;
using HidroRec.Backend.Domain.Entities;
using HidroRec.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HidroRec.Backend.Infrastructure.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<HidroRecDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await context.Database.MigrateAsync();

        if (await context.Perfis.AnyAsync())
        {
            return;
        }

        var perfilAdmin = new Perfil { Nome = "Administrador", Descricao = "Acesso total ao sistema" };
        var perfilGestor = new Perfil { Nome = "Gestor", Descricao = "Acesso a operacoes e moderacao" };
        var perfilCidadao = new Perfil { Nome = "Cidadao", Descricao = "Usuario colaborativo" };

        await context.Perfis.AddRangeAsync(perfilAdmin, perfilGestor, perfilCidadao);

        var permissaoReportes = new Permissao { Nome = "Gerenciar reportes", Codigo = "reportes.manage" };
        var permissaoAdmin = new Permissao { Nome = "Ver dashboard admin", Codigo = "admin.read" };
        var permissaoUsuarios = new Permissao { Nome = "Gerenciar usuarios", Codigo = "users.manage" };

        await context.Permissoes.AddRangeAsync(permissaoReportes, permissaoAdmin, permissaoUsuarios);

        var regiaoNorte = new Regiao { Nome = "Norte" };
        var regiaoCentro = new Regiao { Nome = "Centro" };
        var regiaoSul = new Regiao { Nome = "Sul" };
        var regiaoOeste = new Regiao { Nome = "Oeste" };

        await context.Regioes.AddRangeAsync(regiaoNorte, regiaoCentro, regiaoSul, regiaoOeste);

        var bairroCasaForte = new Bairro { Nome = "Casa Forte", Regiao = regiaoNorte, Latitude = -8.0263m, Longitude = -34.9147m };
        var bairroEspinheiro = new Bairro { Nome = "Espinheiro", Regiao = regiaoCentro, Latitude = -8.0450m, Longitude = -34.8921m };
        var bairroCentro = new Bairro { Nome = "Centro", Regiao = regiaoCentro, Latitude = -8.0634m, Longitude = -34.8751m };
        var bairroBoaViagem = new Bairro { Nome = "Boa Viagem", Regiao = regiaoSul, Latitude = -8.1261m, Longitude = -34.9017m };
        var bairroMadalena = new Bairro { Nome = "Madalena", Regiao = regiaoOeste, Latitude = -8.0515m, Longitude = -34.9094m };

        await context.Bairros.AddRangeAsync(bairroCasaForte, bairroEspinheiro, bairroCentro, bairroBoaViagem, bairroMadalena);

        var fonteApac = new FonteDado { Nome = "APAC", Tipo = "Clima", Descricao = "Leituras de chuva e previsao." };
        var fontePorto = new FonteDado { Nome = "Porto do Recife", Tipo = "Mare", Descricao = "Indicadores de variacao de mare." };

        await context.FontesDado.AddRangeAsync(fonteApac, fontePorto);
        await context.SaveChangesAsync();

        await context.PerfilPermissoes.AddRangeAsync(
            new PerfilPermissao { PerfilId = perfilAdmin.Id, PermissaoId = permissaoReportes.Id },
            new PerfilPermissao { PerfilId = perfilAdmin.Id, PermissaoId = permissaoAdmin.Id },
            new PerfilPermissao { PerfilId = perfilAdmin.Id, PermissaoId = permissaoUsuarios.Id },
            new PerfilPermissao { PerfilId = perfilGestor.Id, PermissaoId = permissaoReportes.Id },
            new PerfilPermissao { PerfilId = perfilGestor.Id, PermissaoId = permissaoAdmin.Id });

        var admin = new Usuario
        {
            Nome = "Operacao HidroRec",
            Email = "admin@hidrorec.local",
            SenhaHash = passwordHasher.Hash("HidroRec#2026"),
            Telefone = "(81) 99999-2026",
            PerfilId = perfilAdmin.Id,
            Perfil = perfilAdmin
        };

        var gestor = new Usuario
        {
            Nome = "Monitoramento Recife",
            Email = "gestor@hidrorec.local",
            SenhaHash = passwordHasher.Hash("HidroRec#2026"),
            Telefone = "(81) 98888-2026",
            PerfilId = perfilGestor.Id,
            Perfil = perfilGestor
        };

        await context.Usuarios.AddRangeAsync(admin, gestor);
        await context.SaveChangesAsync();

        var reportes = new[]
        {
            new Reporte
            {
                Titulo = "Alagamento na Av. Caxanga",
                Descricao = "Faixa da direita tomada por agua, trafego comprometido.",
                NivelAgua = NivelAgua.Cintura,
                TipoOcorrencia = TipoOcorrencia.Alagamento,
                Severidade = SeveridadeReporte.Alagamento,
                Status = StatusReporte.Confirmado,
                Latitude = -8.0390m,
                Longitude = -34.9480m,
                EnderecoReferencia = "Proximo ao Shopping RioMar",
                BairroId = bairroMadalena.Id,
                RegiaoId = regiaoOeste.Id,
                BairroNome = bairroMadalena.Nome,
                RegiaoNome = regiaoOeste.Nome,
                NomeUsuario = "Equipe de Campo",
                ContatoUsuario = "(81) 98800-1000",
                Fonte = "Operacional",
                Observacoes = "Equipe recomendou desvio local.",
                UsuarioId = admin.Id,
                DataOcorrencia = DateTime.UtcNow.AddMinutes(-55)
            },
            new Reporte
            {
                Titulo = "Ponto de atencao na Av. Agamenon Magalhaes",
                Descricao = "Acumulo de agua no canteiro central e reducao de velocidade.",
                NivelAgua = NivelAgua.Joelho,
                TipoOcorrencia = TipoOcorrencia.PontoAtencao,
                Severidade = SeveridadeReporte.Atencao,
                Status = StatusReporte.EmAnalise,
                Latitude = -8.0415m,
                Longitude = -34.8908m,
                EnderecoReferencia = "Espinheiro",
                BairroId = bairroEspinheiro.Id,
                RegiaoId = regiaoCentro.Id,
                BairroNome = bairroEspinheiro.Nome,
                RegiaoNome = regiaoCentro.Nome,
                NomeUsuario = "Painel Viario",
                ContatoUsuario = "(81) 97777-2200",
                Fonte = "Colaborativa",
                UsuarioId = gestor.Id,
                DataOcorrencia = DateTime.UtcNow.AddMinutes(-42)
            },
            new Reporte
            {
                Titulo = "Rua da Aurora com lamina d'agua",
                Descricao = "Trecho central com agua acima da roda de veiculos pequenos.",
                NivelAgua = NivelAgua.Cintura,
                TipoOcorrencia = TipoOcorrencia.Alagamento,
                Severidade = SeveridadeReporte.Alagamento,
                Status = StatusReporte.Confirmado,
                Latitude = -8.0585m,
                Longitude = -34.8788m,
                EnderecoReferencia = "Rua da Aurora",
                BairroId = bairroCentro.Id,
                RegiaoId = regiaoCentro.Id,
                BairroNome = bairroCentro.Nome,
                RegiaoNome = regiaoCentro.Nome,
                NomeUsuario = "Monitor Urbano",
                ContatoUsuario = "(81) 96666-3300",
                Fonte = "Colaborativa",
                UsuarioId = gestor.Id,
                DataOcorrencia = DateTime.UtcNow.AddMinutes(-35)
            },
            new Reporte
            {
                Titulo = "Ponto de atencao na Av. Norte",
                Descricao = "Drenagem lenta e agua na lateral da pista.",
                NivelAgua = NivelAgua.Tornozelo,
                TipoOcorrencia = TipoOcorrencia.Microalagamento,
                Severidade = SeveridadeReporte.Atencao,
                Status = StatusReporte.Pendente,
                Latitude = -8.0276m,
                Longitude = -34.9135m,
                EnderecoReferencia = "Casa Forte",
                BairroId = bairroCasaForte.Id,
                RegiaoId = regiaoNorte.Id,
                BairroNome = bairroCasaForte.Nome,
                RegiaoNome = regiaoNorte.Nome,
                NomeUsuario = "Colaborador Urbano",
                ContatoUsuario = "(81) 95555-4400",
                Fonte = "Colaborativa",
                DataOcorrencia = DateTime.UtcNow.AddMinutes(-18)
            },
            new Reporte
            {
                Titulo = "Pocas na orla",
                Descricao = "Acumulo localizado sem bloqueio viario.",
                NivelAgua = NivelAgua.Pocas,
                TipoOcorrencia = TipoOcorrencia.Microalagamento,
                Severidade = SeveridadeReporte.Normal,
                Status = StatusReporte.Resolvido,
                Latitude = -8.1290m,
                Longitude = -34.9002m,
                EnderecoReferencia = "Calcada da praia de Boa Viagem",
                BairroId = bairroBoaViagem.Id,
                RegiaoId = regiaoSul.Id,
                BairroNome = bairroBoaViagem.Nome,
                RegiaoNome = regiaoSul.Nome,
                NomeUsuario = "Equipe de Limpeza",
                ContatoUsuario = "(81) 94444-5500",
                Fonte = "Institucional",
                DataOcorrencia = DateTime.UtcNow.AddHours(-2)
            },
            new Reporte
            {
                Titulo = "Pocas em via local",
                Descricao = "Agua acumulada em rua secundaria sem risco elevado.",
                NivelAgua = NivelAgua.Pocas,
                TipoOcorrencia = TipoOcorrencia.PontoAtencao,
                Severidade = SeveridadeReporte.Normal,
                Status = StatusReporte.Resolvido,
                Latitude = -8.0459m,
                Longitude = -34.9074m,
                EnderecoReferencia = "Rua secundaria da Madalena",
                BairroId = bairroMadalena.Id,
                RegiaoId = regiaoOeste.Id,
                BairroNome = bairroMadalena.Nome,
                RegiaoNome = regiaoOeste.Nome,
                NomeUsuario = "Observador Urbano",
                ContatoUsuario = "(81) 93333-6600",
                Fonte = "Colaborativa",
                DataOcorrencia = DateTime.UtcNow.AddHours(-3)
            }
        };

        await context.Reportes.AddRangeAsync(reportes);
        await context.SaveChangesAsync();

        var alerta1 = new Alerta
        {
            Titulo = "Alagamento ativo em corredor oeste",
            Descricao = "Acumulo critico de agua na Av. Caxanga com impacto viario.",
            Criticidade = CriticidadeAlerta.Alagamento,
            AreaAfetada = "Av. Caxanga / Madalena",
            OrientacaoResumida = "Evite o trecho e utilize desvios pela Torre.",
            BairroId = bairroMadalena.Id
        };

        var alerta2 = new Alerta
        {
            Titulo = "Atencao ampliada no Centro",
            Descricao = "Rua da Aurora apresenta recorrencia de transbordamento e lentidao.",
            Criticidade = CriticidadeAlerta.Atencao,
            AreaAfetada = "Centro / Rua da Aurora",
            OrientacaoResumida = "Motoristas devem reduzir velocidade e evitar retornos alagados.",
            BairroId = bairroCentro.Id
        };

        var indicadores = new[]
        {
            new IndicadorClimatico
            {
                Categoria = "Chuva",
                Titulo = "Volume de chuva",
                Valor = 12m,
                Unidade = "mm/h",
                Status = "Moderada",
                Descricao = "Diminuiu em 30min",
                ReferenciaEm = DateTime.UtcNow,
                FonteDadoId = fonteApac.Id
            },
            new IndicadorClimatico
            {
                Categoria = "Mare",
                Titulo = "Mare atual",
                Valor = 2.3m,
                Unidade = "m",
                Status = "Alta",
                Descricao = "Proxima mudanca: 14:35",
                ReferenciaEm = DateTime.UtcNow,
                FonteDadoId = fontePorto.Id
            }
        };

        var historicos = reportes.Select(reporte => new HistoricoReporte
        {
            ReporteId = reporte.Id,
            StatusAnterior = StatusReporte.Pendente,
            StatusNovo = reporte.Status,
            Observacao = "Seed inicial do MVP.",
            AlteradoPorUsuarioId = admin.Id
        }).ToList();

        await context.Alertas.AddRangeAsync(alerta1, alerta2);
        await context.SaveChangesAsync();

        await context.AlertasReportes.AddRangeAsync(
            new AlertaReporte { AlertaId = alerta1.Id, ReporteId = reportes[0].Id },
            new AlertaReporte { AlertaId = alerta2.Id, ReporteId = reportes[2].Id });

        await context.HistoricosReporte.AddRangeAsync(historicos);
        await context.IndicadoresClimaticos.AddRangeAsync(indicadores);
        await context.Auditorias.AddAsync(new Auditoria
        {
            Entidade = "Sistema",
            EntidadeId = "seed",
            Acao = "Bootstrap",
            UsuarioId = admin.Id,
            Detalhes = "Carga inicial do MVP HidroRec."
        });
        await context.LogsSistema.AddAsync(new LogSistema
        {
            Nivel = "Information",
            Evento = "SeedInicial",
            Mensagem = "Base inicial do HidroRec provisionada.",
            Contexto = nameof(DatabaseInitializer)
        });

        await context.SaveChangesAsync();
    }
}
