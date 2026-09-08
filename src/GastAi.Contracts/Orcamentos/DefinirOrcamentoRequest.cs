namespace GastAi.Contracts.Orcamentos;

/// <summary>CategoriaId nulo define o orçamento geral do mês.</summary>
public record DefinirOrcamentoRequest(int Ano, int Mes, int? CategoriaId, decimal ValorLimite);
