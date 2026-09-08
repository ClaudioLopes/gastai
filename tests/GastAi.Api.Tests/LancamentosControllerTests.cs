using System.Net;
using System.Net.Http.Json;
using GastAi.Contracts.Lancamentos;
using GastAi.Domain.Enums;

namespace GastAi.Api.Tests;

public class LancamentosControllerTests(GastAiApiFactory factory)
    : AuthenticatedApiTestBase(factory), IClassFixture<GastAiApiFactory>
{
    private const int CategoriaAlimentacaoId = 1;
    private const int CategoriaSalarioId = 7;

    [Fact]
    public async Task RegistrarGasto_Valido_RetornaOkComDto()
    {
        var request = new CriarGastoRequest(45.90m, new DateOnly(2031, 1, 10), "Mercado", CategoriaAlimentacaoId, FormaPagamento.Pix, null);

        var response = await Client.PostAsJsonAsync("/api/lancamentos/gastos", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<LancamentoDto>();
        Assert.NotNull(dto);
        Assert.Equal(TipoLancamento.Gasto, dto!.Tipo);
        Assert.Equal("Alimentação", dto.CategoriaNome);
        Assert.Equal(FormaPagamento.Pix, dto.FormaPagamento);
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
        var criado = await (await Client.PostAsJsonAsync("/api/lancamentos/gastos",
                new CriarGastoRequest(10m, new DateOnly(2031, 3, 1), "original", CategoriaAlimentacaoId, FormaPagamento.Dinheiro, null)))
            .Content.ReadFromJsonAsync<LancamentoDto>();

        var atualizarRequest = new AtualizarLancamentoRequest(99.90m, new DateOnly(2031, 3, 2), "atualizado", CategoriaAlimentacaoId, FormaPagamento.Credito, "obs");
        var response = await Client.PutAsJsonAsync($"/api/lancamentos/{criado!.Id}", atualizarRequest);

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
        var criado = await (await Client.PostAsJsonAsync("/api/lancamentos/gastos",
                new CriarGastoRequest(10m, new DateOnly(2031, 4, 1), "a excluir", CategoriaAlimentacaoId, FormaPagamento.Pix, null)))
            .Content.ReadFromJsonAsync<LancamentoDto>();

        var delete = await Client.DeleteAsync($"/api/lancamentos/{criado!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var lista = await Client.GetFromJsonAsync<List<LancamentoDto>>("/api/lancamentos?ano=2031&mes=4");
        Assert.DoesNotContain(lista!, l => l.Id == criado.Id);
    }

    [Fact]
    public async Task ListarPorMes_RetornaApenasLancamentosDoMesInformado()
    {
        await Client.PostAsJsonAsync("/api/lancamentos/gastos",
            new CriarGastoRequest(10m, new DateOnly(2031, 5, 15), "dentro do mes", CategoriaAlimentacaoId, FormaPagamento.Pix, null));
        await Client.PostAsJsonAsync("/api/lancamentos/gastos",
            new CriarGastoRequest(20m, new DateOnly(2031, 6, 1), "fora do mes", CategoriaAlimentacaoId, FormaPagamento.Pix, null));

        var lista = await Client.GetFromJsonAsync<List<LancamentoDto>>("/api/lancamentos?ano=2031&mes=5");

        Assert.Contains(lista!, l => l.Descricao == "dentro do mes");
        Assert.DoesNotContain(lista!, l => l.Descricao == "fora do mes");
    }
}
