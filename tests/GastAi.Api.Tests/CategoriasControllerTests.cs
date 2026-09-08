using System.Net;
using System.Net.Http.Json;
using GastAi.Contracts.Categorias;
using GastAi.Domain.Enums;

namespace GastAi.Api.Tests;

public class CategoriasControllerTests(GastAiApiFactory factory)
    : AuthenticatedApiTestBase(factory), IClassFixture<GastAiApiFactory>
{
    [Fact]
    public async Task Listar_RetornaAsNoveCategoriasPadraoSeedadas()
    {
        var categorias = await Client.GetFromJsonAsync<List<CategoriaDto>>("/api/categorias?incluirInativas=true");

        Assert.NotNull(categorias);
        Assert.True(categorias!.Count >= 9);
        Assert.Contains(categorias, c => c.Nome == "Alimentação" && c.Tipo == TipoLancamento.Gasto && c.PadraoDoSistema);
        Assert.Contains(categorias, c => c.Nome == "Salário" && c.Tipo == TipoLancamento.Receita && c.PadraoDoSistema);
    }

    [Fact]
    public async Task Listar_FiltrandoPorTipo_RetornaApenasDoTipoPedido()
    {
        var categorias = await Client.GetFromJsonAsync<List<CategoriaDto>>("/api/categorias?tipo=Receita");

        Assert.NotNull(categorias);
        Assert.All(categorias!, c => Assert.Equal(TipoLancamento.Receita, c.Tipo));
    }

    [Fact]
    public async Task Criar_CategoriaNova_RetornaOkComCategoriaCustomizada()
    {
        var response = await Client.PostAsJsonAsync("/api/categorias", new CriarCategoriaRequest("Assinaturas", TipoLancamento.Gasto, "#000000"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var categoria = await response.Content.ReadFromJsonAsync<CategoriaDto>();
        Assert.NotNull(categoria);
        Assert.Equal("Assinaturas", categoria!.Nome);
        Assert.False(categoria.PadraoDoSistema);
    }

    [Fact]
    public async Task Criar_CategoriaComNomeDuplicado_RetornaBadRequest()
    {
        await Client.PostAsJsonAsync("/api/categorias", new CriarCategoriaRequest("Streaming", TipoLancamento.Gasto, null));

        var response = await Client.PostAsJsonAsync("/api/categorias", new CriarCategoriaRequest("Streaming", TipoLancamento.Gasto, null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ExcluirCategoriaPadrao_RetornaBadRequest()
    {
        var categorias = await Client.GetFromJsonAsync<List<CategoriaDto>>("/api/categorias?tipo=Gasto");
        var padrao = categorias!.First(c => c.PadraoDoSistema);

        var response = await Client.DeleteAsync($"/api/categorias/{padrao.Id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DesativarEReativarCategoria_RemoveEDevolveNaListaDeAtivas()
    {
        var criada = await (await Client.PostAsJsonAsync("/api/categorias", new CriarCategoriaRequest("Temporária", TipoLancamento.Gasto, null)))
            .Content.ReadFromJsonAsync<CategoriaDto>();

        var desativar = await Client.PostAsync($"/api/categorias/{criada!.Id}/desativar", null);
        Assert.Equal(HttpStatusCode.NoContent, desativar.StatusCode);

        var ativasSemInativa = await Client.GetFromJsonAsync<List<CategoriaDto>>("/api/categorias?tipo=Gasto");
        Assert.DoesNotContain(ativasSemInativa!, c => c.Id == criada.Id);

        var reativar = await Client.PostAsync($"/api/categorias/{criada.Id}/reativar", null);
        Assert.Equal(HttpStatusCode.NoContent, reativar.StatusCode);

        var ativasComReativada = await Client.GetFromJsonAsync<List<CategoriaDto>>("/api/categorias?tipo=Gasto");
        Assert.Contains(ativasComReativada!, c => c.Id == criada.Id);
    }
}
