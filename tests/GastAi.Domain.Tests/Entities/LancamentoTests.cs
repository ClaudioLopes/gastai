using GastAi.Domain.Entities;
using GastAi.Domain.Enums;
using GastAi.Domain.Exceptions;

namespace GastAi.Domain.Tests.Entities;

public class LancamentoTests
{
    private static readonly DateOnly Hoje = new(2026, 9, 4);

    [Fact]
    public void NovoGasto_ComDadosValidos_CriaComTipoGasto()
    {
        var lancamento = Lancamento.NovoGasto(45.90m, Hoje, "Mercado", categoriaId: 1, FormaPagamento.Pix);

        Assert.Equal(TipoLancamento.Gasto, lancamento.Tipo);
        Assert.Equal(45.90m, lancamento.Valor);
        Assert.Equal(Hoje, lancamento.Data);
        Assert.Equal("Mercado", lancamento.Descricao);
        Assert.Equal(1, lancamento.CategoriaId);
        Assert.Equal(FormaPagamento.Pix, lancamento.FormaPagamento);
    }

    [Fact]
    public void NovaReceita_ComDadosValidos_CriaSemFormaPagamento()
    {
        var lancamento = Lancamento.NovaReceita(3000m, Hoje, "Salário", categoriaId: 7);

        Assert.Equal(TipoLancamento.Receita, lancamento.Tipo);
        Assert.Null(lancamento.FormaPagamento);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void NovoGasto_ValorNaoPositivo_Lanca(decimal valor)
    {
        Assert.Throws<DomainException>(() => Lancamento.NovoGasto(valor, Hoje, "x", 1, FormaPagamento.Pix));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void NovaReceita_ValorNaoPositivo_Lanca(decimal valor)
    {
        Assert.Throws<DomainException>(() => Lancamento.NovaReceita(valor, Hoje, "x", 1));
    }

    [Fact]
    public void Atualizar_GastoSemFormaPagamento_Lanca()
    {
        var lancamento = Lancamento.NovoGasto(10m, Hoje, "x", 1, FormaPagamento.Pix);

        Assert.Throws<DomainException>(() =>
            lancamento.Atualizar(20m, Hoje, "y", 1, formaPagamento: null, observacao: null));
    }

    [Fact]
    public void Atualizar_ReceitaComFormaPagamento_Lanca()
    {
        var lancamento = Lancamento.NovaReceita(10m, Hoje, "x", 7);

        Assert.Throws<DomainException>(() =>
            lancamento.Atualizar(20m, Hoje, "y", 7, formaPagamento: FormaPagamento.Pix, observacao: null));
    }

    [Fact]
    public void Atualizar_ComDadosValidos_AlteraCampos()
    {
        var lancamento = Lancamento.NovoGasto(10m, Hoje, "x", 1, FormaPagamento.Dinheiro);
        var novaData = Hoje.AddDays(1);

        lancamento.Atualizar(99.90m, novaData, "Atualizado", 2, FormaPagamento.Credito, "obs");

        Assert.Equal(99.90m, lancamento.Valor);
        Assert.Equal(novaData, lancamento.Data);
        Assert.Equal("Atualizado", lancamento.Descricao);
        Assert.Equal(2, lancamento.CategoriaId);
        Assert.Equal(FormaPagamento.Credito, lancamento.FormaPagamento);
        Assert.Equal("obs", lancamento.Observacao);
    }
}
