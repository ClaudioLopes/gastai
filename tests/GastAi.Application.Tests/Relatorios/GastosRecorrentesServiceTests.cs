using GastAi.Application.Abstractions;
using GastAi.Application.Relatorios;
using GastAi.Application.Tests.TestUtils;
using GastAi.Contracts.Relatorios;
using GastAi.Domain.Entities;
using GastAi.Domain.Enums;
using Moq;

namespace GastAi.Application.Tests.Relatorios;

public class GastosRecorrentesServiceTests
{
    private readonly Mock<ILancamentoRepository> _lancamentos = new();
    private readonly GastosRecorrentesService _service;
    private readonly Categoria _moradia = new("Moradia", TipoLancamento.Gasto);

    public GastosRecorrentesServiceTests()
    {
        _service = new GastosRecorrentesService(_lancamentos.Object);
    }

    private Lancamento Gasto(decimal valor, int ano, int mes, string descricao, Guid? grupoParcelamento = null)
    {
        var lancamento = Lancamento.NovoGasto(valor, new DateOnly(ano, mes, 10), descricao, 1, FormaPagamento.Pix);
        if (grupoParcelamento is not null)
            lancamento.MarcarComoParcela(grupoParcelamento.Value, 1, 3);

        EntidadeTestHelpers.AnexarCategoria(lancamento, _moradia);
        return lancamento;
    }

    private void DadoLancamentos(params Lancamento[] itens)
        => _lancamentos
            .Setup(r => r.GetByPeriodoAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([.. itens]);

    [Fact]
    public async Task ListarAsync_GastoQueSeRepete_CalculaVariacaoEmRelacaoAoMesAnterior()
    {
        DadoLancamentos(
            Gasto(100m, 2026, 7, "Conta de luz"),
            Gasto(110m, 2026, 8, "Conta de luz"),
            Gasto(132m, 2026, 9, "Conta de luz"));

        var resultado = await _service.ListarAsync(2026, 9);

        var luz = Assert.Single(resultado);
        Assert.Equal(132m, luz.ValorAtual);
        Assert.Equal(110m, luz.ValorAnterior);
        Assert.Equal(20m, luz.VariacaoPercentual);
        Assert.Equal(TendenciaGasto.Subindo, luz.Tendencia);
        Assert.Equal(6, luz.Serie.Count);
        Assert.Equal([0m, 0m, 0m, 100m, 110m, 132m], luz.Serie.Select(s => s.Valor));
    }

    [Fact]
    public async Task ListarAsync_ValorCaindo_ClassificaComoCaindo()
    {
        DadoLancamentos(
            Gasto(200m, 2026, 8, "Internet"),
            Gasto(150m, 2026, 9, "Internet"));

        var resultado = await _service.ListarAsync(2026, 9);

        var internet = Assert.Single(resultado);
        Assert.Equal(-25m, internet.VariacaoPercentual);
        Assert.Equal(TendenciaGasto.Caindo, internet.Tendencia);
    }

    [Fact]
    public async Task ListarAsync_VariacaoPequena_ClassificaComoEstavel()
    {
        DadoLancamentos(
            Gasto(100m, 2026, 8, "Aluguel"),
            Gasto(101m, 2026, 9, "Aluguel"));

        var resultado = await _service.ListarAsync(2026, 9);

        Assert.Equal(TendenciaGasto.Estavel, Assert.Single(resultado).Tendencia);
    }

    [Fact]
    public async Task ListarAsync_GastoComUmUnicoMes_NaoEhRecorrente()
    {
        DadoLancamentos(Gasto(100m, 2026, 9, "Conserto do carro"));

        var resultado = await _service.ListarAsync(2026, 9);

        Assert.Empty(resultado);
    }

    [Fact]
    public async Task ListarAsync_IgnoraCaixaAcentosEEspacosNaDescricao()
    {
        DadoLancamentos(
            Gasto(100m, 2026, 8, "Conta de Luz"),
            Gasto(110m, 2026, 9, "  conta  de luz "),
            Gasto(50m, 2026, 9, "Água"),
            Gasto(60m, 2026, 8, "agua"));

        var resultado = await _service.ListarAsync(2026, 9);

        Assert.Equal(2, resultado.Count);
        Assert.Contains(resultado, r => r.Descricao == "conta  de luz" && r.ValorAtual == 110m && r.ValorAnterior == 100m);
        Assert.Contains(resultado, r => r.Descricao == "Água" && r.ValorAtual == 50m && r.ValorAnterior == 60m);
    }

    [Fact]
    public async Task ListarAsync_IgnoraParcelasDeComprasParceladasEGastosSemDescricao()
    {
        var grupo = Guid.NewGuid();
        DadoLancamentos(
            Gasto(100m, 2026, 8, "Notebook (1/3)", grupo),
            Gasto(100m, 2026, 9, "Notebook (1/3)", grupo),
            Gasto(30m, 2026, 8, ""),
            Gasto(30m, 2026, 9, ""));

        var resultado = await _service.ListarAsync(2026, 9);

        Assert.Empty(resultado);
    }

    [Fact]
    public async Task ListarAsync_GastoNaoLancadoNoMesSelecionado_FicaSemValorAtualESemVariacao()
    {
        DadoLancamentos(
            Gasto(100m, 2026, 7, "Conta de luz"),
            Gasto(120m, 2026, 8, "Conta de luz"));

        var resultado = await _service.ListarAsync(2026, 9);

        var luz = Assert.Single(resultado);
        Assert.Null(luz.ValorAtual);
        Assert.Equal(120m, luz.ValorAnterior);
        Assert.Null(luz.VariacaoPercentual);
    }

    [Fact]
    public async Task ListarAsync_DoisLancamentosNoMesmoMes_SomaOsValores()
    {
        DadoLancamentos(
            Gasto(100m, 2026, 8, "Conta de luz"),
            Gasto(60m, 2026, 9, "Conta de luz"),
            Gasto(40m, 2026, 9, "Conta de luz"));

        var resultado = await _service.ListarAsync(2026, 9);

        Assert.Equal(100m, Assert.Single(resultado).ValorAtual);
        Assert.Equal(0m, resultado[0].VariacaoPercentual);
    }
}
