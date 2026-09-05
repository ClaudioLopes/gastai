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
    decimal Percentual);

public record ComparacaoMesAnteriorDto(
    bool ExisteDadosMesAnterior,
    decimal TotalGastosMesAnterior,
    decimal? VariacaoPercentualGastos,
    decimal TotalReceitasMesAnterior,
    decimal? VariacaoPercentualReceitas,
    decimal SaldoMesAnterior);
