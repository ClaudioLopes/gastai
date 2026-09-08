using System.Net;
using System.Net.Http.Json;
using GastAi.Contracts.Lancamentos;
using GastAi.Contracts.Orcamentos;
using GastAi.Domain.Enums;

namespace GastAi.Api.Tests;

public class OrcamentosControllerTests(GastAiApiFactory factory)
    : AuthenticatedApiTestBase(factory), IClassFixture<GastAiApiFactory>
{
    private const int CategoriaAlimentacaoId = 1;
    private const int CategoriaSalarioId = 7;

    [Fact]
    public async Task Definir_OrcamentoGeral_CriaERetornaDto()
    {
        var response = await Client.PutAsJsonAsync("/api/orcamentos", new DefinirOrcamentoRequest(2041, 1, null, 2000m));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<OrcamentoMensalDto>();
        Assert.Null(dto!.CategoriaId);
        Assert.Equal("Geral", dto.CategoriaNome);
        Assert.Equal(2000m, dto.ValorLimite);
    }

    [Fact]
    public async Task Definir_OrcamentoParaCategoriaDeReceita_RetornaBadRequest()
    {
        var response = await Client.PutAsJsonAsync("/api/orcamentos", new DefinirOrcamentoRequest(2041, 1, CategoriaSalarioId, 500m));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Definir_ChamadoDuasVezesParaMesmaCategoriaEMes_AtualizaEmVezDeDuplicar()
    {
        await Client.PutAsJsonAsync("/api/orcamentos", new DefinirOrcamentoRequest(2041, 2, CategoriaAlimentacaoId, 300m));
        await Client.PutAsJsonAsync("/api/orcamentos", new DefinirOrcamentoRequest(2041, 2, CategoriaAlimentacaoId, 450m));

        var lista = await Client.GetFromJsonAsync<List<OrcamentoMensalDto>>("/api/orcamentos?ano=2041&mes=2");

        var doCategoria = lista!.Where(o => o.CategoriaId == CategoriaAlimentacaoId).ToList();
        Assert.Single(doCategoria);
        Assert.Equal(450m, doCategoria[0].ValorLimite);
    }

    [Fact]
    public async Task Listar_ComGastosLancados_CalculaValorGastoEPercentual()
    {
        await Client.PutAsJsonAsync("/api/orcamentos", new DefinirOrcamentoRequest(2041, 3, CategoriaAlimentacaoId, 200m));
        await Client.PostAsJsonAsync("/api/lancamentos/gastos",
            new CriarGastoRequest(50m, new DateOnly(2041, 3, 10), "mercado", CategoriaAlimentacaoId, FormaPagamento.Pix, null));

        var lista = await Client.GetFromJsonAsync<List<OrcamentoMensalDto>>("/api/orcamentos?ano=2041&mes=3");

        var doCategoria = lista!.Single(o => o.CategoriaId == CategoriaAlimentacaoId);
        Assert.Equal(50m, doCategoria.ValorGasto);
        Assert.Equal(25m, doCategoria.PercentualConsumido);
    }

    [Fact]
    public async Task Excluir_OrcamentoExistente_RemoveDaListagem()
    {
        var criado = await (await Client.PutAsJsonAsync("/api/orcamentos", new DefinirOrcamentoRequest(2041, 4, CategoriaAlimentacaoId, 100m)))
            .Content.ReadFromJsonAsync<OrcamentoMensalDto>();

        var delete = await Client.DeleteAsync($"/api/orcamentos/{criado!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var lista = await Client.GetFromJsonAsync<List<OrcamentoMensalDto>>("/api/orcamentos?ano=2041&mes=4");
        Assert.DoesNotContain(lista!, o => o.Id == criado.Id);
    }

    [Fact]
    public async Task Excluir_OrcamentoInexistente_RetornaNotFound()
    {
        var response = await Client.DeleteAsync("/api/orcamentos/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
