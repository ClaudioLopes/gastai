using GastAi.Application.Abstractions;
using GastAi.Application.Lancamentos;
using GastAi.Application.Tests.TestUtils;
using GastAi.Contracts.Lancamentos;
using GastAi.Domain.Entities;
using GastAi.Domain.Enums;
using GastAi.Domain.Exceptions;
using Moq;

namespace GastAi.Application.Tests.Lancamentos;

public class CompraAutomaticaServiceTests
{
    private readonly Mock<ILancamentoRepository> _lancamentos = new();
    private readonly Mock<ICategoriaRepository> _categorias = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Categoria _outros = CategoriaComId("Outros", 6);
    private readonly Categoria _alimentacao = CategoriaComId("Alimentação", 1);
    private Lancamento? _salvo;

    public CompraAutomaticaServiceTests()
    {
        _categorias
            .Setup(r => r.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync([_alimentacao, _outros]);

        _lancamentos
            .Setup(r => r.AddAsync(It.IsAny<Lancamento>(), It.IsAny<CancellationToken>()))
            .Callback<Lancamento, CancellationToken>((l, _) => _salvo = l)
            .Returns(Task.CompletedTask);
    }

    private static Categoria CategoriaComId(string nome, int id, TipoLancamento tipo = TipoLancamento.Gasto)
    {
        var categoria = new Categoria(nome, tipo);
        EntidadeTestHelpers.DefinirId(categoria, id);
        return categoria;
    }

    private CompraAutomaticaService CriarServico(DateTimeOffset? agoraUtc = null)
        => new(_lancamentos.Object, _categorias.Object, _unitOfWork.Object,
            new RelogioFixo(agoraUtc ?? new DateTimeOffset(2026, 11, 9, 15, 0, 0, TimeSpan.Zero)));

    [Fact]
    public async Task RegistrarAsync_SemHistorico_UsaCategoriaOutrosEMarcaAConfirmar()
    {
        var resultado = await CriarServico().RegistrarAsync(new CompraAutomaticaRequest(45.90m, "Padaria Central"));

        Assert.Equal(6, resultado.CategoriaId);
        Assert.True(resultado.AConfirmar);
        Assert.Equal(TipoLancamento.Gasto, resultado.Tipo);
        Assert.Equal(FormaPagamento.Credito, resultado.FormaPagamento);
        Assert.Equal("Padaria Central", resultado.Descricao);
        Assert.NotNull(_salvo);
        Assert.True(_salvo!.AConfirmar);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarAsync_ComHistoricoDoEstabelecimento_ReaproveitaACategoria()
    {
        _lancamentos
            .Setup(r => r.GetCategoriaDaUltimaCompraAsync("IFOOD", It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var resultado = await CriarServico().RegistrarAsync(new CompraAutomaticaRequest(30m, "  IFOOD "));

        Assert.Equal(1, resultado.CategoriaId);
        Assert.Equal("Alimentação", resultado.CategoriaNome);
    }

    [Fact]
    public async Task RegistrarAsync_HistoricoApontaParaCategoriaInativa_CaiEmOutros()
    {
        _lancamentos
            .Setup(r => r.GetCategoriaDaUltimaCompraAsync("Uber", It.IsAny<CancellationToken>()))
            .ReturnsAsync(99);

        var resultado = await CriarServico().RegistrarAsync(new CompraAutomaticaRequest(20m, "Uber"));

        Assert.Equal(6, resultado.CategoriaId);
    }

    [Fact]
    public async Task RegistrarAsync_SemEstabelecimento_UsaDescricaoGenerica()
    {
        var resultado = await CriarServico().RegistrarAsync(new CompraAutomaticaRequest(10m, "  "));

        Assert.Equal("Compra no cartão", resultado.Descricao);
    }

    [Fact]
    public async Task RegistrarAsync_SemData_UsaDataDeBrasiliaEntrandoNoDiaAnterior()
    {
        // 01:30 UTC de 10/11 ainda é 22:30 de 09/11 em Brasília.
        var servico = CriarServico(new DateTimeOffset(2026, 11, 10, 1, 30, 0, TimeSpan.Zero));

        var resultado = await servico.RegistrarAsync(new CompraAutomaticaRequest(10m, "Cafe"));

        Assert.Equal(new DateOnly(2026, 11, 9), resultado.Data);
    }

    [Fact]
    public async Task RegistrarAsync_ComDataECartao_RespeitaDataEGuardaCartaoNaObservacao()
    {
        var resultado = await CriarServico().RegistrarAsync(
            new CompraAutomaticaRequest(10m, "Cafe", new DateOnly(2026, 11, 1), "Nubank", FormaPagamento.Debito));

        Assert.Equal(new DateOnly(2026, 11, 1), resultado.Data);
        Assert.Equal(FormaPagamento.Debito, resultado.FormaPagamento);
        Assert.Equal("Registrada automaticamente (Nubank)", resultado.Observacao);
    }

    [Fact]
    public async Task RegistrarAsync_ValorZeroOuNegativo_Falha()
    {
        await Assert.ThrowsAsync<DomainException>(
            () => CriarServico().RegistrarAsync(new CompraAutomaticaRequest(0m, "Cafe")));
    }

    [Fact]
    public async Task RegistrarAsync_SemNenhumaCategoriaDeGasto_Falha()
    {
        _categorias
            .Setup(r => r.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync([CategoriaComId("Salário", 7, TipoLancamento.Receita)]);

        await Assert.ThrowsAsync<DomainException>(
            () => CriarServico().RegistrarAsync(new CompraAutomaticaRequest(10m, "Cafe")));
    }

    private sealed class RelogioFixo(DateTimeOffset agora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => agora;
    }
}
