using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace GastAi.Api.Tests;

/// <summary>
/// Sobe a Api completa (DI, migrations, filtros de exceção) contra um arquivo
/// SQLite temporário e isolado por classe de teste — mesma fiação usada em produção.
/// </summary>
public class GastAiApiFactory : WebApplicationFactory<Program>
{
    public const string OwnerEmail = "owner@gastai.test";
    public const string OwnerPassword = "Teste!12345678";

    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"gastai-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_dbPath}",
                ["Auth:OwnerEmail"] = OwnerEmail,
                ["Auth:OwnerPassword"] = OwnerPassword,
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        foreach (var suffix in new[] { "", "-shm", "-wal" })
        {
            try
            {
                File.Delete(_dbPath + suffix);
            }
            catch (IOException)
            {
                // arquivo pode ainda estar em uso por um handle do SQLite sendo finalizado; sem impacto nos testes.
            }
        }
    }
}
