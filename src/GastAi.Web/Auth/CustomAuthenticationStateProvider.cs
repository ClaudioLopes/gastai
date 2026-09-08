using System.Net.Http.Json;
using System.Security.Claims;
using GastAi.Contracts.Auth;
using Microsoft.AspNetCore.Components.Authorization;

namespace GastAi.Web.Auth;

public class CustomAuthenticationStateProvider(HttpClient http) : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonimo = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var response = await http.GetAsync("api/auth/me");
            if (!response.IsSuccessStatusCode)
                return new AuthenticationState(Anonimo);

            var me = await response.Content.ReadFromJsonAsync<MeResponse>();
            if (me is null)
                return new AuthenticationState(Anonimo);

            var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, me.Email)], authenticationType: "cookie");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch (HttpRequestException)
        {
            return new AuthenticationState(Anonimo);
        }
    }

    public void NotificarMudancaDeEstado() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}
