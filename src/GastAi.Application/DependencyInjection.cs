using GastAi.Application.Categorias;
using GastAi.Application.Lancamentos;
using GastAi.Application.Relatorios;
using Microsoft.Extensions.DependencyInjection;

namespace GastAi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICategoriaService, CategoriaService>();
        services.AddScoped<ILancamentoService, LancamentoService>();
        services.AddScoped<IRelatorioMensalService, RelatorioMensalService>();

        return services;
    }
}
