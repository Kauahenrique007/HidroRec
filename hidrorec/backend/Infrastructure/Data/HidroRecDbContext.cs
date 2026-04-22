using HidroRec.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HidroRec.Backend.Infrastructure.Data;

public sealed class HidroRecDbContext(DbContextOptions<HidroRecDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Perfil> Perfis => Set<Perfil>();
    public DbSet<Permissao> Permissoes => Set<Permissao>();
    public DbSet<PerfilPermissao> PerfilPermissoes => Set<PerfilPermissao>();
    public DbSet<Organizacao> Organizacoes => Set<Organizacao>();
    public DbSet<Regiao> Regioes => Set<Regiao>();
    public DbSet<Bairro> Bairros => Set<Bairro>();
    public DbSet<AreaMonitorada> AreasMonitoradas => Set<AreaMonitorada>();
    public DbSet<AtivoMonitorado> AtivosMonitorados => Set<AtivoMonitorado>();
    public DbSet<FonteDado> FontesDado => Set<FonteDado>();
    public DbSet<IndicadorClimatico> IndicadoresClimaticos => Set<IndicadorClimatico>();
    public DbSet<Reporte> Reportes => Set<Reporte>();
    public DbSet<HistoricoReporte> HistoricosReporte => Set<HistoricoReporte>();
    public DbSet<Alerta> Alertas => Set<Alerta>();
    public DbSet<AlertaReporte> AlertasReportes => Set<AlertaReporte>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();
    public DbSet<LogSistema> LogsSistema => Set<LogSistema>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Regiao>()
            .HasIndex(x => x.Nome)
            .IsUnique();

        modelBuilder.Entity<Organizacao>()
            .HasIndex(x => x.Codigo)
            .IsUnique();

        modelBuilder.Entity<Bairro>()
            .HasIndex(x => new { x.RegiaoId, x.Nome })
            .IsUnique();

        modelBuilder.Entity<AreaMonitorada>()
            .HasIndex(x => x.Codigo)
            .IsUnique();

        modelBuilder.Entity<AtivoMonitorado>()
            .HasIndex(x => x.Codigo)
            .IsUnique();

        modelBuilder.Entity<PerfilPermissao>()
            .HasKey(x => new { x.PerfilId, x.PermissaoId });

        modelBuilder.Entity<AlertaReporte>()
            .HasKey(x => new { x.AlertaId, x.ReporteId });

        modelBuilder.Entity<Usuario>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<Usuario>()
            .HasIndex(x => new { x.PerfilId, x.Ativo, x.DataCriacao });

        modelBuilder.Entity<Permissao>()
            .HasIndex(x => x.Codigo)
            .IsUnique();

        modelBuilder.Entity<Reporte>().Property(x => x.NivelAgua).HasConversion<string>();
        modelBuilder.Entity<Reporte>().Property(x => x.TipoOcorrencia).HasConversion<string>();
        modelBuilder.Entity<Reporte>().Property(x => x.Severidade).HasConversion<string>();
        modelBuilder.Entity<Reporte>().Property(x => x.Status).HasConversion<string>();
        modelBuilder.Entity<HistoricoReporte>().Property(x => x.StatusAnterior).HasConversion<string>();
        modelBuilder.Entity<HistoricoReporte>().Property(x => x.StatusNovo).HasConversion<string>();
        modelBuilder.Entity<Alerta>().Property(x => x.Criticidade).HasConversion<string>();
        modelBuilder.Entity<Reporte>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<Alerta>().Property(x => x.RowVersion).IsRowVersion();

        modelBuilder.Entity<Reporte>()
            .HasOne(x => x.Bairro)
            .WithMany(x => x.Reportes)
            .HasForeignKey(x => x.BairroId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Reporte>()
            .HasOne(x => x.Regiao)
            .WithMany()
            .HasForeignKey(x => x.RegiaoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Reporte>()
            .HasOne(x => x.AreaMonitorada)
            .WithMany(x => x.Reportes)
            .HasForeignKey(x => x.AreaMonitoradaId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<AreaMonitorada>()
            .HasOne(x => x.Organizacao)
            .WithMany(x => x.AreasMonitoradas)
            .HasForeignKey(x => x.OrganizacaoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AreaMonitorada>()
            .HasOne(x => x.Bairro)
            .WithMany()
            .HasForeignKey(x => x.BairroId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<AreaMonitorada>()
            .HasOne(x => x.Regiao)
            .WithMany()
            .HasForeignKey(x => x.RegiaoId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<AtivoMonitorado>()
            .HasOne(x => x.Organizacao)
            .WithMany(x => x.AtivosMonitorados)
            .HasForeignKey(x => x.OrganizacaoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AtivoMonitorado>()
            .HasOne(x => x.AreaMonitorada)
            .WithMany(x => x.AtivosMonitorados)
            .HasForeignKey(x => x.AreaMonitoradaId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<HistoricoReporte>()
            .HasOne(x => x.AlteradoPorUsuario)
            .WithMany()
            .HasForeignKey(x => x.AlteradoPorUsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Auditoria>()
            .HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Reporte>().Property(x => x.Latitude).HasPrecision(10, 6);
        modelBuilder.Entity<Reporte>().Property(x => x.Longitude).HasPrecision(10, 6);
        modelBuilder.Entity<Bairro>().Property(x => x.Latitude).HasPrecision(10, 6);
        modelBuilder.Entity<Bairro>().Property(x => x.Longitude).HasPrecision(10, 6);
        modelBuilder.Entity<AreaMonitorada>().Property(x => x.Latitude).HasPrecision(10, 6);
        modelBuilder.Entity<AreaMonitorada>().Property(x => x.Longitude).HasPrecision(10, 6);
        modelBuilder.Entity<AtivoMonitorado>().Property(x => x.Latitude).HasPrecision(10, 6);
        modelBuilder.Entity<AtivoMonitorado>().Property(x => x.Longitude).HasPrecision(10, 6);
        modelBuilder.Entity<IndicadorClimatico>().Property(x => x.Valor).HasPrecision(10, 2);

        modelBuilder.Entity<Reporte>()
            .HasIndex(x => new { x.Excluido, x.DataOcorrencia, x.Status, x.Severidade })
            .HasDatabaseName("IX_Reportes_DashboardLookup");

        modelBuilder.Entity<Reporte>()
            .HasIndex(x => new { x.Excluido, x.BairroId, x.DataOcorrencia })
            .HasDatabaseName("IX_Reportes_Bairro_DataOcorrencia");

        modelBuilder.Entity<Reporte>()
            .HasIndex(x => new { x.Excluido, x.RegiaoId, x.DataOcorrencia })
            .HasDatabaseName("IX_Reportes_Regiao_DataOcorrencia");

        modelBuilder.Entity<Reporte>()
            .HasIndex(x => new { x.Excluido, x.UsuarioId, x.DataCriacao })
            .HasDatabaseName("IX_Reportes_Usuario_DataCriacao");

        modelBuilder.Entity<Reporte>()
            .HasIndex(x => new { x.Excluido, x.AreaMonitoradaId, x.DataOcorrencia })
            .HasDatabaseName("IX_Reportes_AreaMonitorada_DataOcorrencia");

        modelBuilder.Entity<AreaMonitorada>()
            .HasIndex(x => new { x.Ativa, x.OrganizacaoId, x.RegiaoId })
            .HasDatabaseName("IX_AreasMonitoradas_Ativa_Organizacao_Regiao");

        modelBuilder.Entity<AtivoMonitorado>()
            .HasIndex(x => new { x.Ativo, x.OrganizacaoId, x.AreaMonitoradaId })
            .HasDatabaseName("IX_AtivosMonitorados_Ativo_Organizacao_Area");

        modelBuilder.Entity<HistoricoReporte>()
            .HasIndex(x => new { x.ReporteId, x.DataAlteracao })
            .HasDatabaseName("IX_HistoricosReporte_Reporte_DataAlteracao");

        modelBuilder.Entity<Alerta>()
            .HasIndex(x => new { x.Ativo, x.Criticidade, x.BairroId, x.DataCriacao })
            .HasDatabaseName("IX_Alertas_Ativo_Criticidade_Bairro_DataCriacao");

        modelBuilder.Entity<Auditoria>()
            .HasIndex(x => new { x.Entidade, x.EntidadeId, x.DataCriacao })
            .HasDatabaseName("IX_Auditorias_Entidade_EntidadeId_DataCriacao");

        modelBuilder.Entity<Auditoria>()
            .HasIndex(x => new { x.UsuarioId, x.DataCriacao })
            .HasDatabaseName("IX_Auditorias_Usuario_DataCriacao");

        modelBuilder.Entity<LogSistema>()
            .HasIndex(x => new { x.Nivel, x.Evento, x.DataCriacao })
            .HasDatabaseName("IX_LogsSistema_Nivel_Evento_DataCriacao");

        modelBuilder.Entity<LogSistema>()
            .HasIndex(x => new { x.Contexto, x.DataCriacao })
            .HasDatabaseName("IX_LogsSistema_Contexto_DataCriacao");

        modelBuilder.Entity<IndicadorClimatico>()
            .HasIndex(x => new { x.FonteDadoId, x.Categoria, x.ReferenciaEm })
            .HasDatabaseName("IX_IndicadoresClimaticos_Fonte_Categoria_Referencia");
    }
}
