using Microsoft.AspNetCore.Identity;

namespace GastAi.Infrastructure.Identity;

/// <summary>
/// App de uso individual: existe exatamente um usuário (o dono), criado no
/// startup a partir de configuração — não há endpoint de registro público.
/// </summary>
public class ApplicationUser : IdentityUser;
