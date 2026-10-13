using System.Net.Http.Json;
using GastAi.Contracts.Lancamentos;
using GastAi.Contracts.Relatorios;
using GastAi.Domain.Enums;

namespace GastAi.Api.Tests;

public class RelatoriosControllerTests(GastAiApiFactory factory)
    : AuthenticatedApiTestBase(factory), IClassFixture<GastAiApiFactory>
{
    private const int CategoriaAlimentacaoId = 1;
    private const int CategoriaTransporteId = 2;
    private const int CategoriaSalarioId = 7;

    [Fact]
    public async Task Mensal_ComLancamentosNoMesENoAnterior_CalculaTotaisEComparacao()
    {
        // Mês anterior (agosto/2032): só gasto, sem receita.
        await Client.PostAsJsonAsync("/api/lancamentos/gastos",
            new CriarGastoRequest(100m, new DateOnly(2032, 8, 10), "x", CategoriaAlimentacaoId, FormaPagamento.Pix, null));

        // Mês corrente (setembro/2032): dois gastos e uma receita.
        await Client.PostAsJsonAsync("/api/lancamentos/gastos",
            new CriarGastoRequest(75m, new DateOnly(2032, 9, 5), "mercado", CategoriaAlimentacaoId, FormaPagamento.Pix, null));
        await Client.PostAsJsonAsync("/api/lancamentos/gastos",
            new CriarGastoRequest(25m, new DateOnly(2032, 9, 6), "onibus", CategoriaTransporteId, FormaPagamento.Debito, null));
        await Client.PostAsJsonAsync("/api/lancamentos/receitas",
            new CriarReceitaRequest(1000m, new DateOnly(2032, 9, 1), "salário", CategoriaSalarioId, null));

        var relatorio = await Client.GetFromJsonAsync<RelatorioMensalDto>("/api/relatorios/mensal?ano=2032&mes=9");

        Assert.NotNull(relatorio);
        Assert.Equal(1000m, relatorio!.TotalReceitas);
        Assert.Equal(100m, relatorio.TotalGastos);
        Assert.Equal(900m, relatorio.Saldo);
        Assert.Equal(2, relatorio.GastosPorCategoria.Count);

        Assert.True(relatorio.ComparacaoMesAnterior.ExisteDadosMesAnterior);
        Assert.Equal(100m, relatorio.ComparacaoMesAnterior.TotalGastosMesAnterior);
        Assert.Equal(0m, relatorio.ComparacaoMesAnterior.VariacaoPercentualGastos);
        Assert.Null(relatorio.ComparacaoMesAnterior.VariacaoPercentualReceitas);
    }

    [Fact]
    public async Task Recorrentes_GastoComMesmaDescricaoEmDoisMeses_RetornaComVariacao()
    {
        await Client.PostAsJsonAsync("/api/lancamentos/gastos",
            new CriarGastoRequest(100m, new DateOnly(2034, 7, 10), "Conta de luz", CategoriaAlimentacaoId, FormaPagamento.Pix, null));
        await Client.PostAsJsonAsync("/api/lancamentos/gastos",
            new CriarGastoRequest(125m, new DateOnly(2034, 8, 10), "conta de luz", CategoriaAlimentacaoId, FormaPagamento.Pix, null));
        await Client.PostAsJsonAsync("/api/lancamentos/gastos",
            new CriarGastoRequest(30m, new DateOnly(2034, 8, 12), "Almoço único", CategoriaAlimentacaoId, FormaPagamento.Pix, null));

        var recorrentes = await Client.GetFromJsonAsync<List<GastoRecorrenteDto>>("/api/relatorios/recorrentes?ano=2034&mes=8");

        Assert.NotNull(recorrentes);
        var luz = Assert.Single(recorrentes!);
        Assert.Equal(125m, luz.ValorAtual);
        Assert.Equal(100m, luz.ValorAnterior);
        Assert.Equal(25m, luz.VariacaoPercentual);
        Assert.Equal(TendenciaGasto.Subindo, luz.Tendencia);
        Assert.Equal(6, luz.Serie.Count);
    }

    [Fact]
    public async Task Mensal_SemLancamentos_RetornaTotaisZeradosESemComparacao()
    {
        var relatorio = await Client.GetFromJsonAsync<RelatorioMensalDto>("/api/relatorios/mensal?ano=2033&mes=1");

        Assert.NotNull(relatorio);
        Assert.Equal(0m, relatorio!.TotalReceitas);
        Assert.Equal(0m, relatorio.TotalGastos);
        Assert.Equal(0m, relatorio.Saldo);
        Assert.Empty(relatorio.GastosPorCategoria);
        Assert.False(relatorio.ComparacaoMesAnterior.ExisteDadosMesAnterior);
    }
}
