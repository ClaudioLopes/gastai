using GastAi.Domain.Enums;

namespace GastAi.Contracts.Lancamentos;

public record AtualizarLancamentoRequest(
    decimal Valor,
    DateOnly Data,
    string Descricao,
    int CategoriaId,
    FormaPagamento? FormaPagamento,
    string? Observacao);
