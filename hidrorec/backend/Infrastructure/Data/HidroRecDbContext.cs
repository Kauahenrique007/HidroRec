using HidroRec.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HidroRec.Backend.Infrastructure.Data;

public sealed class HidroRecDbContext(DbContextOptions<HidroRecDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Perfil> Perfis => Set<Perfil>();
    public DbSet<Permissao> Permissoes => Set<Permissao>();
    public DbSet<PerfilPermissao> PerfilPermissoes => Set<PerfilPermissao>();
    public DbSet<Regiao> Regioes => Set<Regiao>();
    public DbSet<Bairro> Bairros => Set<Bairro>();
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

        modelBuilder.Entity<PerfilPermissao>()
            .HasKey(x => new { x.PerfilId, x.PermissaoId });

        modelBuilder.Entity<AlertaReporte>()
            .HasKey(x => new { x.AlertaId, x.ReporteId });

        modelBuilder.Entity<Usuario>()
            .HasIndex(x => x.Email)
            .IsUnique();

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
        modelBuilder.Entity<IndicadorClimatico>().Property(x => x.Valor).HasPrecision(10, 2);
    }
}
