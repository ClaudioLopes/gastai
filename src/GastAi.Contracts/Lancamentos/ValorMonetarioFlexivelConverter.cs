using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GastAi.Contracts.Lancamentos;

/// <summary>
/// Aceita o valor como número JSON (45.9) ou como texto ("45,90", "R$ 1.234,56"), porque
/// automações como o app Atalhos do iPhone enviam o valor da compra já formatado como texto.
/// </summary>
public class ValorMonetarioFlexivelConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
            return reader.GetDecimal();

        if (reader.TokenType == JsonTokenType.String && TentarInterpretar(reader.GetString(), out var valor))
            return valor;

        throw new JsonException("Valor monetário inválido.");
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);

    /// <summary>
    /// Interpreta textos como "45,90", "45.90", "1.234,56", "1,234.56" e "R$ 1.234". O último
    /// separador seguido de 1 ou 2 dígitos é o decimal; qualquer outro "." ou "," é de milhar.
    /// </summary>
    public static bool TentarInterpretar(string? texto, out decimal valor)
    {
        valor = 0;

        if (string.IsNullOrWhiteSpace(texto))
            return false;

        var limpo = new string(texto.Where(c => char.IsDigit(c) || c is '.' or ',').ToArray());
        if (limpo.Length == 0 || !limpo.Any(char.IsDigit))
            return false;

        var ultimoSeparador = limpo.LastIndexOfAny(['.', ',']);
        if (ultimoSeparador >= 0)
        {
            var digitosDepois = limpo.Length - ultimoSeparador - 1;
            var inteiro = limpo[..ultimoSeparador].Replace(".", "").Replace(",", "");

            limpo = digitosDepois is 1 or 2
                ? $"{inteiro}.{limpo[(ultimoSeparador + 1)..]}"
                : inteiro + limpo[(ultimoSeparador + 1)..];
        }

        return decimal.TryParse(limpo, NumberStyles.Number, CultureInfo.InvariantCulture, out valor);
    }
}
