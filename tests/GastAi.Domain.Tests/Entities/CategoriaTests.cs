using GastAi.Domain.Entities;
using GastAi.Domain.Enums;
using GastAi.Domain.Exceptions;

namespace GastAi.Domain.Tests.Entities;

public class CategoriaTests
{
    [Fact]
    public void Construtor_DefineCamposCorretamente()
    {
        var categoria = new Categoria("Alimentação", TipoLancamento.Gasto, "#F97316", padraoDoSistema: true);

        Assert.Equal("Alimentação", categoria.Nome);
        Assert.Equal(TipoLancamento.Gasto, categoria.Tipo);
        Assert.Equal("#F97316", categoria.Cor);
        Assert.True(categoria.PadraoDoSistema);
        Assert.True(categoria.Ativa);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Construtor_NomeInvalido_Lanca(string? nome)
    {
        Assert.Throws<DomainException>(() => new Categoria(nome!, TipoLancamento.Gasto));
    }

    [Fact]
    public void AtualizarNome_RemoveEspacosNasPontas()
    {
        var categoria = new Categoria("Lazer", TipoLancamento.Gasto);

        categoria.AtualizarNome("  Viagem  ");

        Assert.Equal("Viagem", categoria.Nome);
    }

    [Fact]
    public void DesativarEReativar_AlteramFlagAtiva()
    {
        var categoria = new Categoria("Lazer", TipoLancamento.Gasto);

        categoria.Desativar();
        Assert.False(categoria.Ativa);

        categoria.Reativar();
        Assert.True(categoria.Ativa);
    }

    [Fact]
    public void Excluir_CategoriaPadraoDoSistema_Lanca()
    {
        var categoria = new Categoria("Alimentação", TipoLancamento.Gasto, padraoDoSistema: true);

        Assert.Throws<DomainException>(() => categoria.Excluir());
    }

    [Fact]
    public void Excluir_CategoriaCustomizada_NaoLanca()
    {
        var categoria = new Categoria("Pets", TipoLancamento.Gasto, padraoDoSistema: false);

        var exception = Record.Exception(() => categoria.Excluir());

        Assert.Null(exception);
    }
}
