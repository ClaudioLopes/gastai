using System.Net.Http.Json;
using GastAi.Contracts.Auth;

namespace GastAi.Api.Tests;

/// <summary>
/// Base para testes que precisam de um HttpClient já autenticado como o dono
/// do app (seedado pela GastAiApiFactory). O cookie de sessão é mantido pelo
/// HttpClient (WebApplicationFactoryClientOptions.HandleCookies = true por padrão).
/// </summary>
public abstract class AuthenticatedApiTestBase : IAsyncLifetime
{
    private readonly GastAiApiFactory _factory;

    protected HttpClient Client { get; private set; } = null!;

    protected AuthenticatedApiTestBase(GastAiApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        Client = _factory.CreateClient();
        var response = await Client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(GastAiApiFactory.OwnerEmail, GastAiApiFactory.OwnerPassword));
        response.EnsureSuccessStatusCode();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
