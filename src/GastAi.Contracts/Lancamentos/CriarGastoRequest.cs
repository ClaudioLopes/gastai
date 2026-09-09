using GastAi.Domain.Enums;

namespace GastAi.Contracts.Lancamentos;

/// <summary>
/// TotalParcelas nulo (ou 1) lança um gasto único; 2 ou mais gera uma parcela por mês,
/// a partir de Data, dividindo Valor entre elas — só permitido com FormaPagamento.Credito.
/// </summary>
public record CriarGastoRequest(
    decimal Valor,
    DateOnly Data,
    string Descricao,
    int CategoriaId,
    FormaPagamento FormaPagamento,
    string? Observacao,
    int? TotalParcelas = null);
