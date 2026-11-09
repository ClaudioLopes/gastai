namespace GastAi.Contracts.Relatorios;

public record RelatorioMensalDto(
    int Ano,
    int Mes,
    decimal TotalReceitas,
    decimal TotalGastos,
    decimal Saldo,
    IReadOnlyList<GastoPorCategoriaDto> GastosPorCategoria,
    ComparacaoMesAnteriorDto ComparacaoMesAnterior);

public record GastoPorCategoriaDto(
    int CategoriaId,
    string CategoriaNome,
    decimal Valor,
    decimal Percentual,
    decimal ValorMesAnterior,
    decimal? VariacaoPercentual,
    IReadOnlyList<VariacaoPorDescricaoDto>? Detalhes = null);

/// <summary>
/// Dentro de uma categoria, quanto cada descrição (ex.: "Mercado", "Restaurante X") pesou neste mês
/// e no anterior — mostra o que explica a alta ou a queda da categoria.
/// </summary>
public record VariacaoPorDescricaoDto(
    string Descricao,
    decimal Valor,
    decimal ValorMesAnterior)
{
    public decimal Diferenca => Valor - ValorMesAnterior;
}

public record ComparacaoMesAnteriorDto(
    bool ExisteDadosMesAnterior,
    decimal TotalGastosMesAnterior,
    decimal? VariacaoPercentualGastos,
    decimal TotalReceitasMesAnterior,
    decimal? VariacaoPercentualReceitas,
    decimal SaldoMesAnterior);
