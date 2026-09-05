using System.Net.Http.Json;
using GastAi.Contracts.Categorias;
using GastAi.Contracts.Lancamentos;
using GastAi.Contracts.Relatorios;
using GastAi.Domain.Enums;

namespace GastAi.Web.Services;

file record ProblemDetailsLike(string? Title, int? Status);

public class ApiClient(HttpClient http)
{
    public async Task<List<CategoriaDto>> GetCategoriasAsync(TipoLancamento? tipo = null, bool incluirInativas = false)
    {
        var query = new List<string>();
        if (tipo is not null) query.Add($"tipo={tipo}");
        if (incluirInativas) query.Add("incluirInativas=true");
        var queryString = query.Count > 0 ? "?" + string.Join("&", query) : string.Empty;

        return await http.GetFromJsonAsync<List<CategoriaDto>>($"api/categorias{queryString}") ?? [];
    }

    public Task<CategoriaDto?> CriarCategoriaAsync(CriarCategoriaRequest request)
        => PostAsync<CriarCategoriaRequest, CategoriaDto>("api/categorias", request);

    public Task DesativarCategoriaAsync(int id)
        => SendAsync(HttpMethod.Post, $"api/categorias/{id}/desativar");

    public async Task<List<LancamentoDto>> GetLancamentosAsync(int ano, int mes)
        => await http.GetFromJsonAsync<List<LancamentoDto>>($"api/lancamentos?ano={ano}&mes={mes}") ?? [];

    public Task<LancamentoDto?> RegistrarGastoAsync(CriarGastoRequest request)
        => PostAsync<CriarGastoRequest, LancamentoDto>("api/lancamentos/gastos", request);

    public Task<LancamentoDto?> RegistrarReceitaAsync(CriarReceitaRequest request)
        => PostAsync<CriarReceitaRequest, LancamentoDto>("api/lancamentos/receitas", request);

    public Task ExcluirLancamentoAsync(int id)
        => SendAsync(HttpMethod.Delete, $"api/lancamentos/{id}");

    public async Task<RelatorioMensalDto?> GetRelatorioMensalAsync(int ano, int mes)
        => await http.GetFromJsonAsync<RelatorioMensalDto>($"api/relatorios/mensal?ano={ano}&mes={mes}");

    private async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest request)
    {
        var response = await http.PostAsJsonAsync(url, request);
        await ThrowIfFailedAsync(response);
        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    private async Task SendAsync(HttpMethod method, string url)
    {
        var response = await http.SendAsync(new HttpRequestMessage(method, url));
        await ThrowIfFailedAsync(response);
    }

    private static async Task ThrowIfFailedAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsLike>();
        throw new ApiException(problem?.Title ?? $"Erro inesperado ({(int)response.StatusCode}).");
    }
}
