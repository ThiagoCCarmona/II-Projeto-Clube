using Clube.Application.Interfaces;
using Clube.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clube.Infrastructure.Persistence;

/// <summary>
/// DbContext principal do sistema com isolamento multi-tenant via Global Query Filters.
/// </summary>
public class ClubeDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public ClubeDbContext(DbContextOptions<ClubeDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Propriedade avaliada dinamicamente em toda query LINQ pelo EF Core para filtrar o tenant atual.
    /// </summary>
    public Guid? CurrentClubeId => _tenantContext.CurrentClubeId;

    public DbSet<Domain.Entities.Clube> Clubes => Set<Domain.Entities.Clube>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Associado> Associados => Set<Associado>();
    public DbSet<Dependente> Dependentes => Set<Dependente>();
    public DbSet<Evento> Eventos => Set<Evento>();
    public DbSet<InscricaoEvento> InscricoesEventos => Set<InscricaoEvento>();
    public DbSet<Espaco> Espacos => Set<Espaco>();
    public DbSet<ReservaEspaco> ReservasEspacos => Set<ReservaEspaco>();
    public DbSet<Mensalidade> Mensalidades => Set<Mensalidade>();
    public DbSet<SolicitacaoReclamacao> SolicitacoesReclamacoes => Set<SolicitacaoReclamacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Domain.Entities.Clube>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(150).IsRequired();
            e.Property(x => x.Sigla).HasMaxLength(20).IsRequired();
            e.Property(x => x.UF).HasMaxLength(2);
            e.Property(x => x.ChavePix).HasMaxLength(120);
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(150).IsRequired();
            e.Property(x => x.CPF).HasMaxLength(14).IsRequired();
            e.HasIndex(x => x.CPF).IsUnique();
        });

        modelBuilder.Entity<Associado>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(150).IsRequired();
            e.Property(x => x.CPF).HasMaxLength(14).IsRequired();
            e.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Clube).WithMany().HasForeignKey(x => x.ClubeId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => CurrentClubeId == null || x.ClubeId == CurrentClubeId);
        });

        modelBuilder.Entity<Dependente>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.AssociadoTitular)
                .WithMany(a => a.Dependentes)
                .HasForeignKey(x => x.AssociadoTitularId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => CurrentClubeId == null || x.ClubeId == CurrentClubeId);
        });

        modelBuilder.Entity<Evento>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Titulo).HasMaxLength(150).IsRequired();
            e.HasQueryFilter(x => CurrentClubeId == null || x.ClubeId == CurrentClubeId);
        });

        modelBuilder.Entity<InscricaoEvento>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Evento).WithMany(ev => ev.Inscricoes).HasForeignKey(x => x.EventoId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Associado).WithMany().HasForeignKey(x => x.AssociadoId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => CurrentClubeId == null || x.ClubeId == CurrentClubeId);
        });

        modelBuilder.Entity<Espaco>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(120).IsRequired();
            e.HasQueryFilter(x => CurrentClubeId == null || x.ClubeId == CurrentClubeId);
        });

        modelBuilder.Entity<ReservaEspaco>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Espaco).WithMany().HasForeignKey(x => x.EspacoId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Associado).WithMany().HasForeignKey(x => x.AssociadoId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => CurrentClubeId == null || x.ClubeId == CurrentClubeId);
        });

        modelBuilder.Entity<Mensalidade>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Associado).WithMany().HasForeignKey(x => x.AssociadoId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => CurrentClubeId == null || x.ClubeId == CurrentClubeId);
        });

        modelBuilder.Entity<SolicitacaoReclamacao>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Associado).WithMany().HasForeignKey(x => x.AssociadoId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => CurrentClubeId == null || x.ClubeId == CurrentClubeId);
        });
    }
}
