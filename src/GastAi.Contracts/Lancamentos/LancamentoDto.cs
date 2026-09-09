using GastAi.Domain.Enums;

namespace GastAi.Contracts.Lancamentos;

public record LancamentoDto(
    int Id,
    TipoLancamento Tipo,
    decimal Valor,
    DateOnly Data,
    string Descricao,
    int CategoriaId,
    string CategoriaNome,
    FormaPagamento? FormaPagamento,
    string? Observacao,
    int? NumeroParcela = null,
    int? TotalParcelas = null);
