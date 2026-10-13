namespace GastAi.Contracts.Relatorios;

public enum TendenciaGasto
{
    Estavel = 0,
    Subindo = 1,
    Caindo = 2
}

public record ValorMensalDto(int Ano, int Mes, decimal Valor);

/// <summary>
/// Gasto que se repete ao longo dos meses (conta de luz, internet...), identificado pela
/// categoria + descrição. <see cref="Serie"/> traz um item por mês da janela analisada,
/// em ordem cronológica, com Valor 0 nos meses em que o gasto não apareceu.
/// </summary>
public record GastoRecorrenteDto(
    int CategoriaId,
    string CategoriaNome,
    string Descricao,
    IReadOnlyList<ValorMensalDto> Serie,
    decimal? ValorAtual,
    decimal? ValorAnterior,
    decimal? VariacaoPercentual,
    TendenciaGasto Tendencia);
