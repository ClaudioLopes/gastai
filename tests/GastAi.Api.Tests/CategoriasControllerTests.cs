using System.Net;
using System.Net.Http.Json;
using GastAi.Contracts.Categorias;
using GastAi.Domain.Enums;

namespace GastAi.Api.Tests;

public class CategoriasControllerTests(GastAiApiFactory factory) : IClassFixture<GastAiApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Listar_RetornaAsNoveCategoriasPadraoSeedadas()
    {
        var categorias = await _client.GetFromJsonAsync<List<CategoriaDto>>("/api/categorias?incluirInativas=true");

        Assert.NotNull(categorias);
        Assert.True(categorias!.Count >= 9);
        Assert.Contains(categorias, c => c.Nome == "Alimentação" && c.Tipo == TipoLancamento.Gasto && c.PadraoDoSistema);
        Assert.Contains(categorias, c => c.Nome == "Salário" && c.Tipo == TipoLancamento.Receita && c.PadraoDoSistema);
    }

    [Fact]
    public async Task Listar_FiltrandoPorTipo_RetornaApenasDoTipoPedido()
    {
        var categorias = await _client.GetFromJsonAsync<List<CategoriaDto>>("/api/categorias?tipo=Receita");

        Assert.NotNull(categorias);
        Assert.All(categorias!, c => Assert.Equal(TipoLancamento.Receita, c.Tipo));
    }

    [Fact]
    public async Task Criar_CategoriaNova_RetornaOkComCategoriaCustomizada()
    {
        var response = await _client.PostAsJsonAsync("/api/categorias", new CriarCategoriaRequest("Assinaturas", TipoLancamento.Gasto, "#000000"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var categoria = await response.Content.ReadFromJsonAsync<CategoriaDto>();
        Assert.NotNull(categoria);
        Assert.Equal("Assinaturas", categoria!.Nome);
        Assert.False(categoria.PadraoDoSistema);
    }

    [Fact]
    public async Task Criar_CategoriaComNomeDuplicado_RetornaBadRequest()
    {
        await _client.PostAsJsonAsync("/api/categorias", new CriarCategoriaRequest("Streaming", TipoLancamento.Gasto, null));

        var response = await _client.PostAsJsonAsync("/api/categorias", new CriarCategoriaRequest("Streaming", TipoLancamento.Gasto, null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ExcluirCategoriaPadrao_RetornaBadRequest()
    {
        var categorias = await _client.GetFromJsonAsync<List<CategoriaDto>>("/api/categorias?tipo=Gasto");
        var padrao = categorias!.First(c => c.PadraoDoSistema);

        var response = await _client.DeleteAsync($"/api/categorias/{padrao.Id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DesativarEReativarCategoria_RemoveEDevolveNaListaDeAtivas()
    {
        var criada = await (await _client.PostAsJsonAsync("/api/categorias", new CriarCategoriaRequest("Temporária", TipoLancamento.Gasto, null)))
            .Content.ReadFromJsonAsync<CategoriaDto>();

        var desativar = await _client.PostAsync($"/api/categorias/{criada!.Id}/desativar", null);
        Assert.Equal(HttpStatusCode.NoContent, desativar.StatusCode);

        var ativasSemInativa = await _client.GetFromJsonAsync<List<CategoriaDto>>("/api/categorias?tipo=Gasto");
        Assert.DoesNotContain(ativasSemInativa!, c => c.Id == criada.Id);

        var reativar = await _client.PostAsync($"/api/categorias/{criada.Id}/reativar", null);
        Assert.Equal(HttpStatusCode.NoContent, reativar.StatusCode);

        var ativasComReativada = await _client.GetFromJsonAsync<List<CategoriaDto>>("/api/categorias?tipo=Gasto");
        Assert.Contains(ativasComReativada!, c => c.Id == criada.Id);
    }
}
