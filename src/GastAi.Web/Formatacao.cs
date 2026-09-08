using System.Globalization;

namespace GastAi.Web;

public static class Formatacao
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static string Moeda(decimal valor) => valor.ToString("C2", PtBr);

    public static string Percentual(decimal? valor) => valor is null ? "—" : $"{valor.Value:+0.#;-0.#;0}%";

    /// <summary>Formata para edição em campo de texto, com vírgula como separador decimal (padrão pt-BR).</summary>
    public static string ParaEdicao(decimal? valor) => valor is null ? string.Empty : valor.Value.ToString("0.##", PtBr);

    /// <summary>
    /// Interpreta o texto digitado pelo usuário aceitando tanto vírgula quanto ponto como
    /// separador decimal — necessário porque &lt;input type="number"&gt; só aceita ponto e
    /// zera o valor ao digitar vírgula, o separador natural em pt-BR.
    /// </summary>
    public static decimal? ParseValorDigitado(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return null;

        var normalizado = texto.Trim().Replace(",", ".");
        return decimal.TryParse(normalizado, NumberStyles.Number, CultureInfo.InvariantCulture, out var valor)
            ? valor
            : null;
    }
}
