using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace GastAi.Infrastructure.Identity;

/// <summary>
/// App de uso individual: em vez de um endpoint de registro público, o único
/// usuário (o dono) é criado no startup a partir de "Auth:OwnerEmail" /
/// "Auth:OwnerPassword". Se essas chaves não estiverem configuradas, nenhum
/// usuário é criado e o login fica indisponível até serem definidas.
/// </summary>
public static class IdentitySeeder
{
    public static async Task SeedOwnerAsync(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        if (userManager.Users.Any())
            return;

        var email = configuration["Auth:OwnerEmail"];
        var password = configuration["Auth:OwnerPassword"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var erros = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Falha ao criar o usuário dono do app: {erros}");
        }
    }
}
