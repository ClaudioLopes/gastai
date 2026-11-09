using System.Net;
using System.Net.Http.Json;
using System.Text;
using GastAi.Contracts.Lancamentos;
using GastAi.Domain.Enums;

namespace GastAi.Api.Tests;

public class ComprasAutomaticasControllerTests(GastAiApiFactory factory)
    : AuthenticatedApiTestBase(factory), IClassFixture<GastAiApiFactory>
{
    private const string Url = "/api/integracoes/compras";
    private const int CategoriaTransporteId = 2;

    /// <summary>Cliente sem cookie de login, como a automação do iPhone: só tem a chave de API.</summary>
    private HttpClient NovoClienteDaAutomacao(string? chave)
    {
        var client = factory.CreateClient();
        if (chave is not null)
            client.DefaultRequestHeaders.Add("X-Api-Key", chave);
        return client;
    }

    private static StringContent Json(string json) => new(json, Encoding.UTF8, "application/json");

    [Fact]
    public async Task Registrar_SemChave_RetornaUnauthorized()
    {
        var response = await NovoClienteDaAutomacao(null).PostAsync(Url, Json("""{"valor":10}"""));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Registrar_ChaveErrada_RetornaUnauthorized()
    {
        var response = await NovoClienteDaAutomacao("chave-errada").PostAsync(Url, Json("""{"valor":10}"""));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Registrar_ChaveCorreta_CriaGastoAConfirmarQueEntraNaListaEPodeSerConfirmado()
    {
        var automacao = NovoClienteDaAutomacao(GastAiApiFactory.ChaveIntegracao);

        var response = await automacao.PostAsync(Url,
            Json("""{"valor":"R$ 45,90","estabelecimento":"Padaria Teste Automatica","data":"2035-03-10","cartao":"Nubank"}"""));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var criado = await response.Content.ReadFromJsonAsync<LancamentoDto>();
        Assert.NotNull(criado);
        Assert.Equal(45.90m, criado!.Valor);
        Assert.Equal(new DateOnly(2035, 3, 10), criado.Data);
        Assert.Equal(FormaPagamento.Credito, criado.FormaPagamento);
        Assert.True(criado.AConfirmar);

        var pendentes = await Client.GetFromJsonAsync<List<LancamentoDto>>("/api/lancamentos/a-confirmar");
        Assert.Contains(pendentes!, p => p.Id == criado.Id);

        var confirmar = await Client.PostAsync($"/api/lancamentos/{criado.Id}/confirmar", content: null);
        Assert.Equal(HttpStatusCode.OK, confirmar.StatusCode);

        var depois = await Client.GetFromJsonAsync<List<LancamentoDto>>("/api/lancamentos/a-confirmar");
        Assert.DoesNotContain(depois!, p => p.Id == criado.Id);

        // Continua existindo no mês, agora já revisado.
        var doMes = await Client.GetFromJsonAsync<List<LancamentoDto>>("/api/lancamentos?ano=2035&mes=3");
        Assert.Contains(doMes!, l => l.Id == criado.Id && !l.AConfirmar);
    }

    [Fact]
    public async Task Registrar_EstabelecimentoJaConhecido_ReaproveitaACategoriaDaCompraRevisada()
    {
        await Client.PostAsJsonAsync("/api/lancamentos/gastos",
            new CriarGastoRequest(20m, new DateOnly(2035, 4, 1), "Posto Teste Aprendizado", CategoriaTransporteId, FormaPagamento.Pix, null));

        var response = await NovoClienteDaAutomacao(GastAiApiFactory.ChaveIntegracao).PostAsync(Url,
            Json("""{"valor":150,"estabelecimento":"Posto Teste Aprendizado","data":"2035-04-05"}"""));

        var criado = await response.Content.ReadFromJsonAsync<LancamentoDto>();
        Assert.Equal(CategoriaTransporteId, criado!.CategoriaId);
        Assert.Equal("Transporte", criado.CategoriaNome);
    }

    [Fact]
    public async Task Registrar_ValorInvalido_RetornaBadRequest()
    {
        var response = await NovoClienteDaAutomacao(GastAiApiFactory.ChaveIntegracao)
            .PostAsync(Url, Json("""{"valor":0,"estabelecimento":"x"}"""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ListarAConfirmar_SemLogin_RetornaUnauthorized()
    {
        var response = await NovoClienteDaAutomacao(GastAiApiFactory.ChaveIntegracao)
            .GetAsync("/api/lancamentos/a-confirmar");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
