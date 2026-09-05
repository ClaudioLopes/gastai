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
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=App_Data/gastai.db";

        services.AddDbContext<GastAiDbContext>(options => options.UseSqlite(connectionString));

        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<ILancamentoRepository, LancamentoRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
