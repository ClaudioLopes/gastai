using System.Text.Json;
using GastAi.Contracts.Lancamentos;

namespace GastAi.Application.Tests.Lancamentos;

public class ValorMonetarioFlexivelConverterTests
{
    [Theory]
    [InlineData("45,90", 45.90)]
    [InlineData("45.90", 45.90)]
    [InlineData("R$ 45,90", 45.90)]
    [InlineData("R$45,90", 45.90)]
    [InlineData("1.234,56", 1234.56)]
    [InlineData("1,234.56", 1234.56)]
    [InlineData("R$ 1.234,56", 1234.56)]
    [InlineData("1.234", 1234)]
    [InlineData("1.234.567,8", 1234567.8)]
    [InlineData("0,5", 0.5)]
    [InlineData("45", 45)]
    [InlineData("  45,9  ", 45.9)]
    public void TentarInterpretar_FormatosComunsDeValor_Converte(string texto, double esperado)
    {
        Assert.True(ValorMonetarioFlexivelConverter.TentarInterpretar(texto, out var valor));
        Assert.Equal((decimal)esperado, valor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("R$")]
    public void TentarInterpretar_TextoSemNumero_Falha(string? texto)
        => Assert.False(ValorMonetarioFlexivelConverter.TentarInterpretar(texto, out _));

    [Theory]
    [InlineData("""{"valor":45.9,"estabelecimento":"Cafe"}""", 45.9)]
    [InlineData("""{"valor":"R$ 45,90","estabelecimento":"Cafe"}""", 45.9)]
    [InlineData("""{"valor":"1.234,56"}""", 1234.56)]
    public void Json_AceitaValorComoNumeroOuTexto(string json, double esperado)
    {
        var request = JsonSerializer.Deserialize<CompraAutomaticaRequest>(
            json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(request);
        Assert.Equal((decimal)esperado, request!.Valor);
    }

    [Fact]
    public void Json_ValorIlegivel_LancaJsonException()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<CompraAutomaticaRequest>(
            """{"valor":"nada"}""", new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    }
}
