namespace GastAi.Contracts.Orcamentos;

public record OrcamentoMensalDto(
    int Id,
    int? CategoriaId,
    string CategoriaNome,
    int Ano,
    int Mes,
    decimal ValorLimite,
    decimal ValorGasto,
    decimal PercentualConsumido);
