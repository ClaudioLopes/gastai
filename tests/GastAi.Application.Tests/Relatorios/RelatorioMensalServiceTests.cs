using GastAi.Application.Abstractions;
using GastAi.Application.Relatorios;
using GastAi.Application.Tests.TestUtils;
using GastAi.Domain.Entities;
using GastAi.Domain.Enums;
using Moq;

namespace GastAi.Application.Tests.Relatorios;

public class RelatorioMensalServiceTests
{
    private readonly Mock<ILancamentoRepository> _lancamentos = new();
    private readonly RelatorioMensalService _service;

    public RelatorioMensalServiceTests()
    {
        _service = new RelatorioMensalService(_lancamentos.Object);
    }

    private static Lancamento Gasto(decimal valor, Categoria categoria, DateOnly data)
    {
        var lancamento = Lancamento.NovoGasto(valor, data, "x", 1, FormaPagamento.Pix);
        EntidadeTestHelpers.AnexarCategoria(lancamento, categoria);
        return lancamento;
    }

    private static Lancamento Receita(decimal valor, Categoria categoria, DateOnly data)
    {
        var lancamento = Lancamento.NovaReceita(valor, data, "x", 7);
        EntidadeTestHelpers.AnexarCategoria(lancamento, categoria);
        return lancamento;
    }

    [Fact]
    public async Task GerarAsync_CalculaTotaisSaldoEGastoPorCategoria()
    {
        var alimentacao = new Categoria("Alimentação", TipoLancamento.Gasto);
        var transporte = new Categoria("Transporte", TipoLancamento.Gasto);
        var salario = new Categoria("Salário", TipoLancamento.Receita);
        EntidadeTestHelpers.DefinirId(alimentacao, 1);
        EntidadeTestHelpers.DefinirId(transporte, 2);
        var data = new DateOnly(2026, 9, 10);

        _lancamentos.Setup(r => r.GetByMesAsync(2026, 9, It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            Gasto(75m, alimentacao, data),
            Gasto(25m, transporte, data),
            Receita(1000m, salario, data),
        ]);
        _lancamentos.Setup(r => r.GetByMesAsync(2026, 8, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var relatorio = await _service.GerarAsync(2026, 9);

        Assert.Equal(1000m, relatorio.TotalReceitas);
        Assert.Equal(100m, relatorio.TotalGastos);
        Assert.Equal(900m, relatorio.Saldo);

        Assert.Equal(2, relatorio.GastosPorCategoria.Count);
        var alimentacaoDto = relatorio.GastosPorCategoria.Single(g => g.CategoriaNome == "Alimentação");
        Assert.Equal(75m, alimentacaoDto.Valor);
        Assert.Equal(75m, alimentacaoDto.Percentual);
        var transporteDto = relatorio.GastosPorCategoria.Single(g => g.CategoriaNome == "Transporte");
        Assert.Equal(25m, transporteDto.Percentual);
    }

    [Fact]
    public async Task GerarAsync_SemLancamentosNoMesAnterior_ComparacaoIndicaAusenciaDeDados()
    {
        _lancamentos.Setup(r => r.GetByMesAsync(2026, 9, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _lancamentos.Setup(r => r.GetByMesAsync(2026, 8, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var relatorio = await _service.GerarAsync(2026, 9);

        Assert.False(relatorio.ComparacaoMesAnterior.ExisteDadosMesAnterior);
        Assert.Null(relatorio.ComparacaoMesAnterior.VariacaoPercentualGastos);
        Assert.Null(relatorio.ComparacaoMesAnterior.VariacaoPercentualReceitas);
    }

    [Fact]
    public async Task GerarAsync_ComDadosMesAnterior_CalculaVariacaoPercentual()
    {
        var categoria = new Categoria("Alimentação", TipoLancamento.Gasto);
        var dataAtual = new DateOnly(2026, 9, 10);
        var dataAnterior = new DateOnly(2026, 8, 10);

        _lancamentos.Setup(r => r.GetByMesAsync(2026, 9, It.IsAny<CancellationToken>()))
            .ReturnsAsync([Gasto(150m, categoria, dataAtual)]);
        _lancamentos.Setup(r => r.GetByMesAsync(2026, 8, It.IsAny<CancellationToken>()))
            .ReturnsAsync([Gasto(100m, categoria, dataAnterior)]);

        var relatorio = await _service.GerarAsync(2026, 9);

        Assert.True(relatorio.ComparacaoMesAnterior.ExisteDadosMesAnterior);
        Assert.Equal(100m, relatorio.ComparacaoMesAnterior.TotalGastosMesAnterior);
        Assert.Equal(50m, relatorio.ComparacaoMesAnterior.VariacaoPercentualGastos);
    }

    [Fact]
    public async Task GerarAsync_ComparaCadaCategoriaComOMesAnterior_InclusiveAsQueZeraramEAsNovas()
    {
        var alimentacao = new Categoria("Alimentação", TipoLancamento.Gasto);
        var transporte = new Categoria("Transporte", TipoLancamento.Gasto);
        var lazer = new Categoria("Lazer", TipoLancamento.Gasto);
        EntidadeTestHelpers.DefinirId(alimentacao, 1);
        EntidadeTestHelpers.DefinirId(transporte, 2);
        EntidadeTestHelpers.DefinirId(lazer, 3);

        _lancamentos.Setup(r => r.GetByMesAsync(2026, 9, It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            Gasto(150m, alimentacao, new DateOnly(2026, 9, 10)),
            Gasto(40m, lazer, new DateOnly(2026, 9, 11)),
        ]);
        _lancamentos.Setup(r => r.GetByMesAsync(2026, 8, It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            Gasto(100m, alimentacao, new DateOnly(2026, 8, 10)),
            Gasto(60m, transporte, new DateOnly(2026, 8, 12)),
        ]);

        var relatorio = await _service.GerarAsync(2026, 9);

        var alimentacaoDto = relatorio.GastosPorCategoria.Single(g => g.CategoriaNome == "Alimentação");
        Assert.Equal(100m, alimentacaoDto.ValorMesAnterior);
        Assert.Equal(50m, alimentacaoDto.VariacaoPercentual);

        var lazerDto = relatorio.GastosPorCategoria.Single(g => g.CategoriaNome == "Lazer");
        Assert.Equal(0m, lazerDto.ValorMesAnterior);
        Assert.Null(lazerDto.VariacaoPercentual);

        var transporteDto = relatorio.GastosPorCategoria.Single(g => g.CategoriaNome == "Transporte");
        Assert.Equal(0m, transporteDto.Valor);
        Assert.Equal(60m, transporteDto.ValorMesAnterior);
        Assert.Equal(-100m, transporteDto.VariacaoPercentual);
    }

    [Fact]
    public async Task GerarAsync_ReceitaMesAnteriorZerada_VariacaoPercentualReceitasFicaNula()
    {
        var salario = new Categoria("Salário", TipoLancamento.Receita);
        var alimentacao = new Categoria("Alimentação", TipoLancamento.Gasto);
        var dataAtual = new DateOnly(2026, 9, 10);
        var dataAnterior = new DateOnly(2026, 8, 10);

        _lancamentos.Setup(r => r.GetByMesAsync(2026, 9, It.IsAny<CancellationToken>()))
            .ReturnsAsync([Receita(1000m, salario, dataAtual)]);
        _lancamentos.Setup(r => r.GetByMesAsync(2026, 8, It.IsAny<CancellationToken>()))
            .ReturnsAsync([Gasto(50m, alimentacao, dataAnterior)]);

        var relatorio = await _service.GerarAsync(2026, 9);

        Assert.Equal(0m, relatorio.ComparacaoMesAnterior.TotalReceitasMesAnterior);
        Assert.Null(relatorio.ComparacaoMesAnterior.VariacaoPercentualReceitas);
    }
}
