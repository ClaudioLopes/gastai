using GastAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GastAi.Infrastructure.Persistence;

public class GastAiDbContext(DbContextOptions<GastAiDbContext> options) : DbContext(options)
{
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Lancamento> Lancamentos => Set<Lancamento>();
    public DbSet<OrcamentoMensal> OrcamentosMensais => Set<OrcamentoMensal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GastAiDbContext).Assembly);
    }
}
