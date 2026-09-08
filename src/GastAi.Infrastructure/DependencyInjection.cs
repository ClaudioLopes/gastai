using GastAi.Application.Abstractions;
using GastAi.Infrastructure.Persistence;
using GastAi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GastAi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Lê a connection string em tempo de resolução (via IServiceProvider), não aqui — em cenários de teste
        // (WebApplicationFactory) a configuração ainda recebe overrides depois deste método rodar e antes do Build().
        services.AddDbContext<GastAiDbContext>((sp, options) =>
        {
            var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection")
                ?? "Data Source=App_Data/gastai.db";

            options.UseSqlite(connectionString);
        });

        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<ILancamentoRepository, LancamentoRepository>();
        services.AddScoped<IOrcamentoRepository, OrcamentoRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
