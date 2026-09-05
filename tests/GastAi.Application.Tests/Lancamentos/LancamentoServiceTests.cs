using GastAi.Application.Abstractions;
using GastAi.Application.Exceptions;
using GastAi.Application.Lancamentos;
using GastAi.Application.Tests.TestUtils;
using GastAi.Contracts.Lancamentos;
using GastAi.Domain.Entities;
using GastAi.Domain.Enums;
using GastAi.Domain.Exceptions;
using Moq;

namespace GastAi.Application.Tests.Lancamentos;

public class LancamentoServiceTests
{
    private static readonly DateOnly Hoje = new(2026, 9, 4);

    private readonly Mock<ILancamentoRepository> _lancamentos = new();
    private readonly Mock<ICategoriaRepository> _categorias = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly LancamentoService _service;

    public LancamentoServiceTests()
    {
        _service = new LancamentoService(_lancamentos.Object, _categorias.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task RegistrarGastoAsync_CategoriaInexistente_LancaNotFoundException()
    {
        _categorias.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Categoria?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.RegistrarGastoAsync(new CriarGastoRequest(10m, Hoje, "x", 1, FormaPagamento.Pix, null)));
    }

    [Fact]
    public async Task RegistrarGastoAsync_CategoriaDeReceita_LancaDomainException()
    {
        var categoriaReceita = new Categoria("Salário", TipoLancamento.Receita);
        _categorias.Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(categoriaReceita);

        await Assert.ThrowsAsync<DomainException>(() =>
            _service.RegistrarGastoAsync(new CriarGastoRequest(10m, Hoje, "x", 7, FormaPagamento.Pix, null)));

        _lancamentos.Verify(r => r.AddAsync(It.IsAny<Lancamento>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegistrarGastoAsync_Valido_RetornaDtoComNomeDaCategoria()
    {
        var categoria = new Categoria("Alimentação", TipoLancamento.Gasto);
        _categorias.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(categoria);

        var dto = await _service.RegistrarGastoAsync(new CriarGastoRequest(45.9m, Hoje, "Mercado", 1, FormaPagamento.Pix, null));

        Assert.Equal("Alimentação", dto.CategoriaNome);
        Assert.Equal(45.9m, dto.Valor);
        Assert.Equal(TipoLancamento.Gasto, dto.Tipo);
        _lancamentos.Verify(r => r.AddAsync(It.IsAny<Lancamento>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarReceitaAsync_CategoriaDeGasto_LancaDomainException()
    {
        var categoriaGasto = new Categoria("Alimentação", TipoLancamento.Gasto);
        _categorias.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(categoriaGasto);

        await Assert.ThrowsAsync<DomainException>(() =>
            _service.RegistrarReceitaAsync(new CriarReceitaRequest(10m, Hoje, "x", 1, null)));
    }

    [Fact]
    public async Task AtualizarAsync_LancamentoInexistente_LancaNotFoundException()
    {
        _lancamentos.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Lancamento?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.AtualizarAsync(1, new AtualizarLancamentoRequest(10m, Hoje, "x", 1, FormaPagamento.Pix, null)));
    }

    [Fact]
    public async Task ExcluirAsync_LancamentoInexistente_LancaNotFoundException()
    {
        _lancamentos.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Lancamento?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.ExcluirAsync(1));
    }

    [Fact]
    public async Task ExcluirAsync_LancamentoExistente_RemoveESalva()
    {
        var lancamento = Lancamento.NovoGasto(10m, Hoje, "x", 1, FormaPagamento.Pix);
        _lancamentos.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(lancamento);

        await _service.ExcluirAsync(1);

        _lancamentos.Verify(r => r.Remove(lancamento), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListarPorMesAsync_OrdenaPorDataDescendente()
    {
        var categoria = new Categoria("Alimentação", TipoLancamento.Gasto);

        var maisAntigo = Lancamento.NovoGasto(10m, Hoje, "antigo", 1, FormaPagamento.Pix);
        var maisRecente = Lancamento.NovoGasto(20m, Hoje.AddDays(1), "recente", 1, FormaPagamento.Pix);
        EntidadeTestHelpers.AnexarCategoria(maisAntigo, categoria);
        EntidadeTestHelpers.AnexarCategoria(maisRecente, categoria);

        _lancamentos.Setup(r => r.GetByMesAsync(2026, 9, It.IsAny<CancellationToken>()))
            .ReturnsAsync([maisAntigo, maisRecente]);

        var resultado = await _service.ListarPorMesAsync(2026, 9);

        Assert.Equal("recente", resultado[0].Descricao);
        Assert.Equal("antigo", resultado[1].Descricao);
    }
}
