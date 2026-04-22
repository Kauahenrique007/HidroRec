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

        if (!await context.Perfis.AnyAsync())
        {
            await SeedBaseAsync(context, passwordHasher);
            return;
        }

        await EnsureOperationalTopologyAsync(context);
        await EnsureReportAreaLinksAsync(context);
    }

    private static async Task SeedBaseAsync(HidroRecDbContext context, IPasswordHasher passwordHasher)
    {
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

        var topology = await EnsureOperationalTopologyAsync(context);
        var areasByCode = topology.Areas.ToDictionary(area => area.Codigo, StringComparer.OrdinalIgnoreCase);

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
                BairroId = topology.Bairros["Madalena"].Id,
                RegiaoId = topology.Regioes["Oeste"].Id,
                AreaMonitoradaId = areasByCode["COR-OESTE-CAXANGA"].Id,
                BairroNome = "Madalena",
                RegiaoNome = "Oeste",
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
                BairroId = topology.Bairros["Espinheiro"].Id,
                RegiaoId = topology.Regioes["Centro"].Id,
                AreaMonitoradaId = areasByCode["SAUDE-AGAMENON"].Id,
                BairroNome = "Espinheiro",
                RegiaoNome = "Centro",
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
                BairroId = topology.Bairros["Centro"].Id,
                RegiaoId = topology.Regioes["Centro"].Id,
                AreaMonitoradaId = areasByCode["DC-CENTRO-AURORA"].Id,
                BairroNome = "Centro",
                RegiaoNome = "Centro",
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
                BairroId = topology.Bairros["Casa Forte"].Id,
                RegiaoId = topology.Regioes["Norte"].Id,
                AreaMonitoradaId = areasByCode["COR-NORTE-AVNORTE"].Id,
                BairroNome = "Casa Forte",
                RegiaoNome = "Norte",
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
                BairroId = topology.Bairros["Boa Viagem"].Id,
                RegiaoId = topology.Regioes["Sul"].Id,
                AreaMonitoradaId = areasByCode["LOG-SUL-BOAVIAGEM"].Id,
                BairroNome = "Boa Viagem",
                RegiaoNome = "Sul",
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
                BairroId = topology.Bairros["Madalena"].Id,
                RegiaoId = topology.Regioes["Oeste"].Id,
                AreaMonitoradaId = areasByCode["COR-OESTE-CAXANGA"].Id,
                BairroNome = "Madalena",
                RegiaoNome = "Oeste",
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
            BairroId = topology.Bairros["Madalena"].Id
        };

        var alerta2 = new Alerta
        {
            Titulo = "Atencao ampliada no Centro",
            Descricao = "Rua da Aurora apresenta recorrencia de transbordamento e lentidao.",
            Criticidade = CriticidadeAlerta.Atencao,
            AreaAfetada = "Centro / Rua da Aurora",
            OrientacaoResumida = "Motoristas devem reduzir velocidade e evitar retornos alagados.",
            BairroId = topology.Bairros["Centro"].Id
        };

        await context.Alertas.AddRangeAsync(alerta1, alerta2);
        await context.SaveChangesAsync();

        await context.AlertasReportes.AddRangeAsync(
            new AlertaReporte { AlertaId = alerta1.Id, ReporteId = reportes[0].Id },
            new AlertaReporte { AlertaId = alerta2.Id, ReporteId = reportes[2].Id });

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
            Observacao = "Seed inicial do HidroRec.",
            AlteradoPorUsuarioId = admin.Id
        }).ToList();

        await context.HistoricosReporte.AddRangeAsync(historicos);
        await context.IndicadoresClimaticos.AddRangeAsync(indicadores);
        await context.Auditorias.AddAsync(new Auditoria
        {
            Entidade = "Sistema",
            EntidadeId = "seed",
            Acao = "Bootstrap",
            UsuarioId = admin.Id,
            Detalhes = "Carga inicial do HidroRec."
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

    private static async Task<(Dictionary<string, Regiao> Regioes, Dictionary<string, Bairro> Bairros, IReadOnlyCollection<AreaMonitorada> Areas)> EnsureOperationalTopologyAsync(HidroRecDbContext context)
    {
        var regioes = await EnsureRegionsAsync(context);
        var bairros = await EnsureNeighborhoodsAsync(context, regioes);
        var organizacoes = await EnsureOrganizationsAsync(context);
        var areas = await EnsureAreasAsync(context, organizacoes, bairros, regioes);
        await EnsureAssetsAsync(context, organizacoes, areas);

        return (regioes, bairros, areas);
    }

    private static async Task<Dictionary<string, Regiao>> EnsureRegionsAsync(HidroRecDbContext context)
    {
        var names = new[] { "Norte", "Centro", "Sul", "Oeste" };
        foreach (var name in names)
        {
            if (!await context.Regioes.AnyAsync(x => x.Nome == name))
            {
                await context.Regioes.AddAsync(new Regiao { Nome = name });
            }
        }

        await context.SaveChangesAsync();
        return await context.Regioes.ToDictionaryAsync(x => x.Nome, StringComparer.OrdinalIgnoreCase);
    }

    private static async Task<Dictionary<string, Bairro>> EnsureNeighborhoodsAsync(HidroRecDbContext context, IReadOnlyDictionary<string, Regiao> regioes)
    {
        var seeds = new[]
        {
            new BairroSeed("Casa Forte", "Norte", -8.0263m, -34.9147m),
            new BairroSeed("Espinheiro", "Centro", -8.0450m, -34.8921m),
            new BairroSeed("Centro", "Centro", -8.0634m, -34.8751m),
            new BairroSeed("Boa Viagem", "Sul", -8.1261m, -34.9017m),
            new BairroSeed("Madalena", "Oeste", -8.0515m, -34.9094m)
        };

        foreach (var seed in seeds)
        {
            if (!await context.Bairros.AnyAsync(x => x.Nome == seed.Nome))
            {
                await context.Bairros.AddAsync(new Bairro
                {
                    Nome = seed.Nome,
                    RegiaoId = regioes[seed.Regiao].Id,
                    Latitude = seed.Latitude,
                    Longitude = seed.Longitude
                });
            }
        }

        await context.SaveChangesAsync();
        return await context.Bairros.Include(x => x.Regiao).ToDictionaryAsync(x => x.Nome, StringComparer.OrdinalIgnoreCase);
    }

    private static async Task<Dictionary<string, Organizacao>> EnsureOrganizationsAsync(HidroRecDbContext context)
    {
        var seeds = new[]
        {
            new Organizacao { Nome = "Centro de Operacoes Recife", Segmento = "Operacao urbana", Tipo = "B2G", Codigo = "COR-REC" },
            new Organizacao { Nome = "Defesa Civil Metropolitana", Segmento = "Resposta territorial", Tipo = "B2G", Codigo = "DCM-REC" },
            new Organizacao { Nome = "Rede Hospitalar Metropolitana", Segmento = "Saude", Tipo = "B2B", Codigo = "RHM-001" },
            new Organizacao { Nome = "Operador Logistico Capibaribe", Segmento = "Logistica", Tipo = "B2B", Codigo = "OLC-001" }
        };

        foreach (var seed in seeds)
        {
            if (!await context.Organizacoes.AnyAsync(x => x.Codigo == seed.Codigo))
            {
                await context.Organizacoes.AddAsync(seed);
            }
        }

        await context.SaveChangesAsync();
        return await context.Organizacoes.ToDictionaryAsync(x => x.Codigo, StringComparer.OrdinalIgnoreCase);
    }

    private static async Task<IReadOnlyCollection<AreaMonitorada>> EnsureAreasAsync(
        HidroRecDbContext context,
        IReadOnlyDictionary<string, Organizacao> organizacoes,
        IReadOnlyDictionary<string, Bairro> bairros,
        IReadOnlyDictionary<string, Regiao> regioes)
    {
        var seeds = new[]
        {
            new AreaSeed("COR-OESTE-CAXANGA", "Corredor Caxanga / Madalena", "Corredor urbano", "COR-REC", "Madalena", "Oeste", -8.0440m, -34.9310m, 84, 86, "Viario e drenagem"),
            new AreaSeed("DC-CENTRO-AURORA", "Eixo Centro / Aurora", "Zona historica", "DCM-REC", "Centro", "Centro", -8.0608m, -34.8784m, 88, 90, "Centro expandido e margem do rio"),
            new AreaSeed("SAUDE-AGAMENON", "Entorno Hospitalar Agamenon", "Area sensivel", "RHM-001", "Espinheiro", "Centro", -8.0420m, -34.8910m, 74, 92, "Hospitais, ambulancias e mobilidade"),
            new AreaSeed("LOG-SUL-BOAVIAGEM", "Orla Sul / Boa Viagem", "Faixa operacional", "OLC-001", "Boa Viagem", "Sul", -8.1252m, -34.8999m, 67, 80, "Rotas, servicos e acessos"),
            new AreaSeed("COR-NORTE-AVNORTE", "Casa Forte / Av. Norte", "Corredor prioritario", "COR-REC", "Casa Forte", "Norte", -8.0285m, -34.9140m, 71, 77, "Drenagem e deslocamento local")
        };

        foreach (var seed in seeds)
        {
            if (!await context.AreasMonitoradas.AnyAsync(x => x.Codigo == seed.Codigo))
            {
                await context.AreasMonitoradas.AddAsync(new AreaMonitorada
                {
                    Codigo = seed.Codigo,
                    Nome = seed.Nome,
                    Tipo = seed.Tipo,
                    OrganizacaoId = organizacoes[seed.OrganizacaoCodigo].Id,
                    BairroId = bairros[seed.Bairro].Id,
                    RegiaoId = regioes[seed.Regiao].Id,
                    Latitude = seed.Latitude,
                    Longitude = seed.Longitude,
                    Vulnerabilidade = seed.Vulnerabilidade,
                    CriticidadeOperacional = seed.CriticidadeOperacional,
                    Cobertura = seed.Cobertura
                });
            }
        }

        await context.SaveChangesAsync();

        return await context.AreasMonitoradas
            .Include(x => x.Organizacao)
            .Include(x => x.Bairro)
            .Include(x => x.Regiao)
            .ToListAsync();
    }

    private static async Task EnsureAssetsAsync(
        HidroRecDbContext context,
        IReadOnlyDictionary<string, Organizacao> organizacoes,
        IReadOnlyCollection<AreaMonitorada> areas)
    {
        var areasByCode = areas.ToDictionary(x => x.Codigo, StringComparer.OrdinalIgnoreCase);

        var seeds = new[]
        {
            new AssetSeed("CD-OESTE", "Centro de Distribuicao Oeste", "Base logistica", "OLC-001", "COR-OESTE-CAXANGA", -8.0445m, -34.9290m, 82, "Contingencia pronta"),
            new AssetSeed("HOSP-ESP", "Hospital Referencia Espinheiro", "Hospital", "RHM-001", "SAUDE-AGAMENON", -8.0434m, -34.8916m, 96, "Monitorado"),
            new AssetSeed("BASE-AURORA", "Base tatica Centro", "Base operacional", "DCM-REC", "DC-CENTRO-AURORA", -8.0599m, -34.8776m, 88, "Monitorado"),
            new AssetSeed("TER-SUL", "Terminal Sul de servicos", "Terminal", "OLC-001", "LOG-SUL-BOAVIAGEM", -8.1245m, -34.9009m, 74, "Operacao assistida"),
            new AssetSeed("NUCLEO-NORTE", "Nucleo de apoio Norte", "Base operacional", "COR-REC", "COR-NORTE-AVNORTE", -8.0290m, -34.9146m, 72, "Monitorado")
        };

        foreach (var seed in seeds)
        {
            if (!await context.AtivosMonitorados.AnyAsync(x => x.Codigo == seed.Codigo))
            {
                await context.AtivosMonitorados.AddAsync(new AtivoMonitorado
                {
                    Codigo = seed.Codigo,
                    Nome = seed.Nome,
                    Tipo = seed.Tipo,
                    OrganizacaoId = organizacoes[seed.OrganizacaoCodigo].Id,
                    AreaMonitoradaId = areasByCode[seed.AreaCodigo].Id,
                    Latitude = seed.Latitude,
                    Longitude = seed.Longitude,
                    Sensibilidade = seed.Sensibilidade,
                    StatusOperacional = seed.StatusOperacional
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task EnsureReportAreaLinksAsync(HidroRecDbContext context)
    {
        var areas = await context.AreasMonitoradas
            .AsNoTracking()
            .Include(x => x.Bairro)
            .Where(x => x.Ativa && !x.Excluido)
            .ToListAsync();

        var reportes = await context.Reportes
            .Where(x => !x.Excluido)
            .ToListAsync();

        var changed = false;
        foreach (var reporte in reportes)
        {
            var resolvedAreaId = ResolveAreaId(areas, reporte);
            if (resolvedAreaId != reporte.AreaMonitoradaId)
            {
                reporte.AreaMonitoradaId = resolvedAreaId;
                changed = true;
            }
        }

        if (changed)
        {
            await context.SaveChangesAsync();
        }
    }

    private static int? ResolveAreaId(IReadOnlyCollection<AreaMonitorada> areas, Reporte reporte)
    {
        var sameNeighborhood = areas
            .Where(area =>
                area.BairroId.HasValue &&
                reporte.BairroId.HasValue &&
                area.BairroId == reporte.BairroId)
            .ToArray();

        var nearestSameNeighborhood = PickNearestArea(sameNeighborhood, reporte.Latitude, reporte.Longitude);
        if (nearestSameNeighborhood is not null)
        {
            return nearestSameNeighborhood.Id;
        }

        var sameNeighborhoodByName = areas
            .Where(area => area.Bairro is not null &&
                           string.Equals(area.Bairro.Nome, reporte.BairroNome, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var nearestByNeighborhoodName = PickNearestArea(sameNeighborhoodByName, reporte.Latitude, reporte.Longitude);
        if (nearestByNeighborhoodName is not null)
        {
            return nearestByNeighborhoodName.Id;
        }

        var sameRegionNearby = areas
            .Where(area =>
                area.RegiaoId.HasValue &&
                reporte.RegiaoId.HasValue &&
                area.RegiaoId == reporte.RegiaoId)
            .ToArray();

        var nearestSameRegion = PickNearestArea(sameRegionNearby, reporte.Latitude, reporte.Longitude, maxDistanceKm: 3.5d);
        if (nearestSameRegion is not null)
        {
            return nearestSameRegion.Id;
        }

        return null;
    }

    private static AreaMonitorada? PickNearestArea(
        IReadOnlyCollection<AreaMonitorada> areas,
        decimal latitude,
        decimal longitude,
        double? maxDistanceKm = null)
    {
        if (areas.Count == 0)
        {
            return null;
        }

        var rankedAreas = areas
            .Select(area => new
            {
                Area = area,
                DistanceKm = CalculateDistanceKm(latitude, longitude, area.Latitude, area.Longitude)
            })
            .OrderBy(x => x.DistanceKm)
            .ThenByDescending(x => x.Area.CriticidadeOperacional)
            .ToArray();

        var nearest = rankedAreas.FirstOrDefault();
        if (nearest is null)
        {
            return null;
        }

        if (maxDistanceKm.HasValue && nearest.DistanceKm > maxDistanceKm.Value)
        {
            return null;
        }

        return nearest.Area;
    }

    private static double CalculateDistanceKm(decimal latitudeA, decimal longitudeA, decimal latitudeB, decimal longitudeB)
    {
        const double earthRadiusKm = 6371d;
        var lat1 = DegreesToRadians((double)latitudeA);
        var lon1 = DegreesToRadians((double)longitudeA);
        var lat2 = DegreesToRadians((double)latitudeB);
        var lon2 = DegreesToRadians((double)longitudeB);

        var deltaLat = lat2 - lat1;
        var deltaLon = lon2 - lon1;

        var sinLat = Math.Sin(deltaLat / 2d);
        var sinLon = Math.Sin(deltaLon / 2d);

        var a = (sinLat * sinLat) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                (sinLon * sinLon);

        var c = 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));
        return earthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * (Math.PI / 180d);

    private sealed record BairroSeed(string Nome, string Regiao, decimal Latitude, decimal Longitude);

    private sealed record AreaSeed(
        string Codigo,
        string Nome,
        string Tipo,
        string OrganizacaoCodigo,
        string Bairro,
        string Regiao,
        decimal Latitude,
        decimal Longitude,
        int Vulnerabilidade,
        int CriticidadeOperacional,
        string Cobertura);

    private sealed record AssetSeed(
        string Codigo,
        string Nome,
        string Tipo,
        string OrganizacaoCodigo,
        string AreaCodigo,
        decimal Latitude,
        decimal Longitude,
        int Sensibilidade,
        string StatusOperacional);
}
