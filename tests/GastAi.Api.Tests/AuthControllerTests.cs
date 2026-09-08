using System.Net;
using System.Net.Http.Json;
using GastAi.Contracts.Auth;

namespace GastAi.Api.Tests;

public class AuthControllerTests(GastAiApiFactory factory) : IClassFixture<GastAiApiFactory>
{
    [Fact]
    public async Task AcessarEndpointProtegido_SemLogin_RetornaUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/categorias");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ComCredenciaisInvalidas_RetornaUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(GastAiApiFactory.OwnerEmail, "SenhaErrada!123"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_PermiteAcessarEndpointProtegido()
    {
        var client = factory.CreateClient();

        var login = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(GastAiApiFactory.OwnerEmail, GastAiApiFactory.OwnerPassword));
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);

        var me = await client.GetFromJsonAsync<MeResponse>("/api/auth/me");
        Assert.Equal(GastAiApiFactory.OwnerEmail, me!.Email);

        var categorias = await client.GetAsync("/api/categorias");
        Assert.Equal(HttpStatusCode.OK, categorias.StatusCode);
    }

    [Fact]
    public async Task Logout_ApagaSessao_EndpointProtegidoVoltaANegar()
    {
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(GastAiApiFactory.OwnerEmail, GastAiApiFactory.OwnerPassword));

        var logout = await client.PostAsync("/api/auth/logout", null);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        var response = await client.GetAsync("/api/categorias");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
