using GastAi.Domain.Entities;
using GastAi.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GastAi.Infrastructure.Persistence;

public class GastAiDbContext(DbContextOptions<GastAiDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Lancamento> Lancamentos => Set<Lancamento>();
    public DbSet<OrcamentoMensal> OrcamentosMensais => Set<OrcamentoMensal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GastAiDbContext).Assembly);
    }
}
