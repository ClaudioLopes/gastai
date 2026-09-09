using System.Net;
using System.Net.Http.Json;
using GastAi.Contracts.Lancamentos;
using GastAi.Contracts.Orcamentos;
using GastAi.Domain.Enums;

namespace GastAi.Api.Tests;

public class LancamentosControllerTests(GastAiApiFactory factory)
    : AuthenticatedApiTestBase(factory), IClassFixture<GastAiApiFactory>
{
    private const int CategoriaAlimentacaoId = 1;
    private const int CategoriaSalarioId = 7;

    [Fact]
    public async Task RegistrarGasto_Valido_RetornaOkComDtoESemAlertas()
    {
        var request = new CriarGastoRequest(45.90m, new DateOnly(2031, 1, 10), "Mercado", CategoriaAlimentacaoId, FormaPagamento.Pix, null);

        var response = await Client.PostAsJsonAsync("/api/lancamentos/gastos", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var resultado = await response.Content.ReadFromJsonAsync<RegistrarGastoResultDto>();
        Assert.NotNull(resultado);
        Assert.Equal(TipoLancamento.Gasto, resultado!.Lancamento.Tipo);
        Assert.Equal("Alimentação", resultado.Lancamento.CategoriaNome);
        Assert.Equal(FormaPagamento.Pix, resultado.Lancamento.FormaPagamento);
        Assert.Empty(resultado.AlertasOrcamento);
    }

    [Fact]
    public async Task RegistrarGasto_UltrapassaOrcamentoDaCategoria_RetornaAlerta()
    {
        await Client.PutAsJsonAsync("/api/orcamentos", new DefinirOrcamentoRequest(2031, 7, CategoriaAlimentacaoId, 100m));

        var request = new CriarGastoRequest(150m, new DateOnly(2031, 7, 10), "Mercado caro", CategoriaAlimentacaoId, FormaPagamento.Pix, null);
        var response = await Client.PostAsJsonAsync("/api/lancamentos/gastos", request);

        var resultado = await response.Content.ReadFromJsonAsync<RegistrarGastoResultDto>();
        Assert.Single(resultado!.AlertasOrcamento);
        Assert.Contains("Alimentação", resultado.AlertasOrcamento[0]);
    }

    [Fact]
    public async Task RegistrarGasto_ValorNegativo_RetornaBadRequest()
    {
        var request = new CriarGastoRequest(-10m, new DateOnly(2031, 1, 10), "x", CategoriaAlimentacaoId, FormaPagamento.Pix, null);

        var response = await Client.PostAsJsonAsync("/api/lancamentos/gastos", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegistrarGasto_CategoriaInexistente_RetornaNotFound()
    {
        var request = new CriarGastoRequest(10m, new DateOnly(2031, 1, 10), "x", 9999, FormaPagamento.Pix, null);

        var response = await Client.PostAsJsonAsync("/api/lancamentos/gastos", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RegistrarGasto_CategoriaDeReceita_RetornaBadRequest()
    {
        var request = new CriarGastoRequest(10m, new DateOnly(2031, 1, 10), "x", CategoriaSalarioId, FormaPagamento.Pix, null);

        var response = await Client.PostAsJsonAsync("/api/lancamentos/gastos", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegistrarGasto_Parcelado_CriaUmaParcelaPorMesEListaNosMesesCorretos()
    {
        var request = new CriarGastoRequest(300m, new DateOnly(2032, 1, 10), "Geladeira", CategoriaAlimentacaoId, FormaPagamento.Credito, null, TotalParcelas: 3);

        var response = await Client.PostAsJsonAsync("/api/lancamentos/gastos", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var resultado = await response.Content.ReadFromJsonAsync<RegistrarGastoResultDto>();
        Assert.Equal(3, resultado!.TotalParcelasGeradas);
        Assert.Equal(100m, resultado.Lancamento.Valor);
        Assert.Equal(1, resultado.Lancamento.NumeroParcela);
        Assert.Equal(3, resultado.Lancamento.TotalParcelas);
        Assert.Equal("Geladeira (1/3)", resultado.Lancamento.Descricao);

        var janeiro = await Client.GetFromJsonAsync<List<LancamentoDto>>("/api/lancamentos?ano=2032&mes=1");
        var fevereiro = await Client.GetFromJsonAsync<List<LancamentoDto>>("/api/lancamentos?ano=2032&mes=2");
        var marco = await Client.GetFromJsonAsync<List<LancamentoDto>>("/api/lancamentos?ano=2032&mes=3");

        Assert.Contains(janeiro!, l => l.Descricao == "Geladeira (1/3)");
        Assert.Contains(fevereiro!, l => l.Descricao == "Geladeira (2/3)");
        Assert.Contains(marco!, l => l.Descricao == "Geladeira (3/3)");
    }

    [Fact]
    public async Task RegistrarGasto_ParceladoSemCredito_RetornaBadRequest()
    {
        var request = new CriarGastoRequest(300m, new DateOnly(2032, 4, 10), "Geladeira", CategoriaAlimentacaoId, FormaPagamento.Pix, null, TotalParcelas: 3);

        var response = await Client.PostAsJsonAsync("/api/lancamentos/gastos", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegistrarReceita_Valida_RetornaOkSemFormaPagamento()
    {
        var request = new CriarReceitaRequest(3000m, new DateOnly(2031, 2, 5), "Salário", CategoriaSalarioId, null);

        var response = await Client.PostAsJsonAsync("/api/lancamentos/receitas", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<LancamentoDto>();
        Assert.Null(dto!.FormaPagamento);
    }

    [Fact]
    public async Task Atualizar_LancamentoExistente_AtualizaCampos()
    {
        var criado = await CriarGastoAsync(new DateOnly(2031, 3, 1), "original", FormaPagamento.Dinheiro);

        var atualizarRequest = new AtualizarLancamentoRequest(99.90m, new DateOnly(2031, 3, 2), "atualizado", CategoriaAlimentacaoId, FormaPagamento.Credito, "obs");
        var response = await Client.PutAsJsonAsync($"/api/lancamentos/{criado.Id}", atualizarRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var atualizado = await response.Content.ReadFromJsonAsync<LancamentoDto>();
        Assert.Equal(99.90m, atualizado!.Valor);
        Assert.Equal("atualizado", atualizado.Descricao);
        Assert.Equal(FormaPagamento.Credito, atualizado.FormaPagamento);
    }

    [Fact]
    public async Task Atualizar_LancamentoInexistente_RetornaNotFound()
    {
        var atualizarRequest = new AtualizarLancamentoRequest(10m, new DateOnly(2031, 3, 1), "x", CategoriaAlimentacaoId, FormaPagamento.Pix, null);

        var response = await Client.PutAsJsonAsync("/api/lancamentos/999999", atualizarRequest);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Excluir_LancamentoExistente_RemoveEDeixaDeAparecerNaListagem()
    {
        var criado = await CriarGastoAsync(new DateOnly(2031, 4, 1), "a excluir", FormaPagamento.Pix);

        var delete = await Client.DeleteAsync($"/api/lancamentos/{criado.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var lista = await Client.GetFromJsonAsync<List<LancamentoDto>>("/api/lancamentos?ano=2031&mes=4");
        Assert.DoesNotContain(lista!, l => l.Id == criado.Id);
    }

    [Fact]
    public async Task ListarPorMes_RetornaApenasLancamentosDoMesInformado()
    {
        await CriarGastoAsync(new DateOnly(2031, 5, 15), "dentro do mes", FormaPagamento.Pix);
        await CriarGastoAsync(new DateOnly(2031, 6, 1), "fora do mes", FormaPagamento.Pix);

        var lista = await Client.GetFromJsonAsync<List<LancamentoDto>>("/api/lancamentos?ano=2031&mes=5");

        Assert.Contains(lista!, l => l.Descricao == "dentro do mes");
        Assert.DoesNotContain(lista!, l => l.Descricao == "fora do mes");
    }

    private async Task<LancamentoDto> CriarGastoAsync(DateOnly data, string descricao, FormaPagamento formaPagamento)
    {
        var response = await Client.PostAsJsonAsync("/api/lancamentos/gastos",
            new CriarGastoRequest(10m, data, descricao, CategoriaAlimentacaoId, formaPagamento, null));
        var resultado = await response.Content.ReadFromJsonAsync<RegistrarGastoResultDto>();
        return resultado!.Lancamento;
    }
}
