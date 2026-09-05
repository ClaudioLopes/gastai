using GastAi.Domain.Enums;

namespace GastAi.Contracts.Lancamentos;

public record CriarGastoRequest(
    decimal Valor,
    DateOnly Data,
    string Descricao,
    int CategoriaId,
    FormaPagamento FormaPagamento,
    string? Observacao);
