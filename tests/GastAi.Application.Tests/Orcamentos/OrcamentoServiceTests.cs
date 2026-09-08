using GastAi.Application.Abstractions;
using GastAi.Application.Exceptions;
using GastAi.Application.Orcamentos;
using GastAi.Application.Tests.TestUtils;
using GastAi.Contracts.Orcamentos;
using GastAi.Domain.Entities;
using GastAi.Domain.Enums;
using GastAi.Domain.Exceptions;
using Moq;

namespace GastAi.Application.Tests.Orcamentos;

public class OrcamentoServiceTests
{
    private static readonly DateOnly MesReferencia = new(2026, 9, 1);

    private readonly Mock<IOrcamentoRepository> _orcamentos = new();
    private readonly Mock<ILancamentoRepository> _lancamentos = new();
    private readonly Mock<ICategoriaRepository> _categorias = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly OrcamentoService _service;

    public OrcamentoServiceTests()
    {
        _service = new OrcamentoService(_orcamentos.Object, _lancamentos.Object, _categorias.Object, _unitOfWork.Object);
    }

    private static Lancamento Gasto(decimal valor, int categoriaId, Categoria categoria)
    {
        var lancamento = Lancamento.NovoGasto(valor, MesReferencia, "x", categoriaId, FormaPagamento.Pix);
        EntidadeTestHelpers.AnexarCategoria(lancamento, categoria);
        return lancamento;
    }

    [Fact]
    public async Task ListarAsync_CalculaValorGastoEPercentualPorOrcamento()
    {
        var alimentacao = new Categoria("Alimentação", TipoLancamento.Gasto);
        var orcamentoCategoria = new OrcamentoMensal(1, MesReferencia, 100m);
        var orcamentoGeral = new OrcamentoMensal(null, MesReferencia, 500m);

        _orcamentos.Setup(r => r.GetByMesAsync(MesReferencia, It.IsAny<CancellationToken>()))
            .ReturnsAsync([orcamentoCategoria, orcamentoGeral]);
        _lancamentos.Setup(r => r.GetByMesAsync(2026, 9, It.IsAny<CancellationToken>()))
            .ReturnsAsync([Gasto(75m, 1, alimentacao)]);

        var resultado = await _service.ListarAsync(2026, 9);

        Assert.Equal(2, resultado.Count);

        var geral = resultado.Single(o => o.CategoriaId is null);
        Assert.Equal("Geral", geral.CategoriaNome);
        Assert.Equal(75m, geral.ValorGasto);
        Assert.Equal(15m, geral.PercentualConsumido);

        var categoria = resultado.Single(o => o.CategoriaId == 1);
        Assert.Equal(75m, categoria.ValorGasto);
        Assert.Equal(75m, categoria.PercentualConsumido);
    }

    [Fact]
    public async Task DefinirAsync_CategoriaInexistente_LancaNotFoundException()
    {
        _categorias.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Categoria?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.DefinirAsync(new DefinirOrcamentoRequest(2026, 9, 1, 100m)));
    }

    [Fact]
    public async Task DefinirAsync_CategoriaDeReceita_LancaDomainException()
    {
        var categoriaReceita = new Categoria("Salário", TipoLancamento.Receita);
        _categorias.Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(categoriaReceita);

        await Assert.ThrowsAsync<DomainException>(() =>
            _service.DefinirAsync(new DefinirOrcamentoRequest(2026, 9, 7, 100m)));
    }

    [Fact]
    public async Task DefinirAsync_SemOrcamentoExistente_CriaNovo()
    {
        var categoria = new Categoria("Alimentação", TipoLancamento.Gasto);
        _categorias.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(categoria);
        _orcamentos.Setup(r => r.GetByCategoriaEMesAsync(1, MesReferencia, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrcamentoMensal?)null);
        _lancamentos.Setup(r => r.GetByMesAsync(2026, 9, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var resultado = await _service.DefinirAsync(new DefinirOrcamentoRequest(2026, 9, 1, 300m));

        Assert.Equal(300m, resultado.ValorLimite);
        Assert.Equal("Alimentação", resultado.CategoriaNome);
        _orcamentos.Verify(r => r.AddAsync(It.IsAny<OrcamentoMensal>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DefinirAsync_OrcamentoJaExistente_AtualizaLimiteSemCriarNovo()
    {
        var existente = new OrcamentoMensal(null, MesReferencia, 500m);
        _orcamentos.Setup(r => r.GetByCategoriaEMesAsync(null, MesReferencia, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existente);
        _lancamentos.Setup(r => r.GetByMesAsync(2026, 9, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var resultado = await _service.DefinirAsync(new DefinirOrcamentoRequest(2026, 9, null, 800m));

        Assert.Equal(800m, resultado.ValorLimite);
        Assert.Equal("Geral", resultado.CategoriaNome);
        _orcamentos.Verify(r => r.AddAsync(It.IsAny<OrcamentoMensal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExcluirAsync_Inexistente_LancaNotFoundException()
    {
        _orcamentos.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((OrcamentoMensal?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.ExcluirAsync(1));
    }

    [Fact]
    public async Task ExcluirAsync_Existente_RemoveESalva()
    {
        var orcamento = new OrcamentoMensal(null, MesReferencia, 500m);
        _orcamentos.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(orcamento);

        await _service.ExcluirAsync(1);

        _orcamentos.Verify(r => r.Remove(orcamento), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerificarAlertasAsync_SemOrcamentoConfigurado_RetornaListaVazia()
    {
        _orcamentos.Setup(r => r.GetByCategoriaEMesAsync(It.IsAny<int?>(), MesReferencia, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrcamentoMensal?)null);
        _lancamentos.Setup(r => r.GetByMesAsync(2026, 9, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var alertas = await _service.VerificarAlertasAsync(1, "Alimentação", 2026, 9);

        Assert.Empty(alertas);
    }

    [Fact]
    public async Task VerificarAlertasAsync_GastoDentroDoLimite_RetornaListaVazia()
    {
        var categoria = new Categoria("Alimentação", TipoLancamento.Gasto);
        var orcamentoCategoria = new OrcamentoMensal(1, MesReferencia, 100m);

        _orcamentos.Setup(r => r.GetByCategoriaEMesAsync(1, MesReferencia, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orcamentoCategoria);
        _orcamentos.Setup(r => r.GetByCategoriaEMesAsync(null, MesReferencia, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrcamentoMensal?)null);
        _lancamentos.Setup(r => r.GetByMesAsync(2026, 9, It.IsAny<CancellationToken>()))
            .ReturnsAsync([Gasto(50m, 1, categoria)]);

        var alertas = await _service.VerificarAlertasAsync(1, "Alimentação", 2026, 9);

        Assert.Empty(alertas);
    }

    [Fact]
    public async Task VerificarAlertasAsync_CategoriaEstourada_RetornaAlerta()
    {
        var categoria = new Categoria("Alimentação", TipoLancamento.Gasto);
        var orcamentoCategoria = new OrcamentoMensal(1, MesReferencia, 100m);

        _orcamentos.Setup(r => r.GetByCategoriaEMesAsync(1, MesReferencia, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orcamentoCategoria);
        _orcamentos.Setup(r => r.GetByCategoriaEMesAsync(null, MesReferencia, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrcamentoMensal?)null);
        _lancamentos.Setup(r => r.GetByMesAsync(2026, 9, It.IsAny<CancellationToken>()))
            .ReturnsAsync([Gasto(150m, 1, categoria)]);

        var alertas = await _service.VerificarAlertasAsync(1, "Alimentação", 2026, 9);

        Assert.Single(alertas);
        Assert.Contains("Alimentação", alertas[0]);
    }

    [Fact]
    public async Task VerificarAlertasAsync_OrcamentoGeralEstourado_RetornaAlerta()
    {
        var categoria = new Categoria("Alimentação", TipoLancamento.Gasto);
        var orcamentoGeral = new OrcamentoMensal(null, MesReferencia, 100m);

        _orcamentos.Setup(r => r.GetByCategoriaEMesAsync(1, MesReferencia, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrcamentoMensal?)null);
        _orcamentos.Setup(r => r.GetByCategoriaEMesAsync(null, MesReferencia, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orcamentoGeral);
        _lancamentos.Setup(r => r.GetByMesAsync(2026, 9, It.IsAny<CancellationToken>()))
            .ReturnsAsync([Gasto(150m, 1, categoria)]);

        var alertas = await _service.VerificarAlertasAsync(1, "Alimentação", 2026, 9);

        Assert.Single(alertas);
        Assert.Contains("geral", alertas[0]);
    }
}
