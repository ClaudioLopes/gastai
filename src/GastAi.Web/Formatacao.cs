using System.Globalization;

namespace GastAi.Web;

public static class Formatacao
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static string Moeda(decimal valor) => valor.ToString("C2", PtBr);

    public static string Percentual(decimal? valor) => valor is null ? "—" : $"{valor.Value:+0.#;-0.#;0}%";
}
