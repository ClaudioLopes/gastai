using GastAi.Application.Abstractions;
using GastAi.Application.Categorias;
using GastAi.Application.Exceptions;
using GastAi.Contracts.Categorias;
using GastAi.Domain.Entities;
using GastAi.Domain.Enums;
using GastAi.Domain.Exceptions;
using Moq;

namespace GastAi.Application.Tests.Categorias;

public class CategoriaServiceTests
{
    private readonly Mock<ICategoriaRepository> _categorias = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly CategoriaService _service;

    public CategoriaServiceTests()
    {
        _service = new CategoriaService(_categorias.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task ListarAsync_FiltraPorTipoEOrdenaPorNome()
    {
        var categorias = new List<Categoria>
        {
            new("Transporte", TipoLancamento.Gasto),
            new("Alimentação", TipoLancamento.Gasto),
            new("Salário", TipoLancamento.Receita),
        };
        _categorias.Setup(r => r.GetAllAsync(false, It.IsAny<CancellationToken>())).ReturnsAsync(categorias);

        var resultado = await _service.ListarAsync(TipoLancamento.Gasto, incluirInativas: false);

        Assert.Equal(2, resultado.Count);
        Assert.Equal("Alimentação", resultado[0].Nome);
        Assert.Equal("Transporte", resultado[1].Nome);
    }

    [Fact]
    public async Task CriarAsync_NomeDuplicado_LancaDomainException()
    {
        _categorias.Setup(r => r.ExisteComNomeAsync("Pets", TipoLancamento.Gasto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<DomainException>(() =>
            _service.CriarAsync(new CriarCategoriaRequest("Pets", TipoLancamento.Gasto, null)));

        _categorias.Verify(r => r.AddAsync(It.IsAny<Categoria>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CriarAsync_NomeDisponivel_AdicionaESalva()
    {
        _categorias.Setup(r => r.ExisteComNomeAsync("Pets", TipoLancamento.Gasto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var resultado = await _service.CriarAsync(new CriarCategoriaRequest("Pets", TipoLancamento.Gasto, "#000"));

        Assert.Equal("Pets", resultado.Nome);
        Assert.False(resultado.PadraoDoSistema);
        _categorias.Verify(r => r.AddAsync(It.IsAny<Categoria>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DesativarAsync_CategoriaInexistente_LancaNotFoundException()
    {
        _categorias.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Categoria?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.DesativarAsync(99));
    }

    [Fact]
    public async Task ExcluirAsync_CategoriaPadraoDoSistema_LancaDomainException()
    {
        var categoria = new Categoria("Alimentação", TipoLancamento.Gasto, padraoDoSistema: true);
        _categorias.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(categoria);

        await Assert.ThrowsAsync<DomainException>(() => _service.ExcluirAsync(1));

        _categorias.Verify(r => r.Remove(It.IsAny<Categoria>()), Times.Never);
    }
}
