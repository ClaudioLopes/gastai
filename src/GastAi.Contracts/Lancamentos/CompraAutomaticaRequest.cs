using System.Text.Json.Serialization;
using GastAi.Domain.Enums;

namespace GastAi.Contracts.Lancamentos;

/// <summary>
/// Compra enviada por uma automação externa (Atalhos do iPhone). Só o valor é obrigatório:
/// sem data vale hoje (horário de Brasília), sem forma de pagamento vale crédito.
/// </summary>
public record CompraAutomaticaRequest(
    [property: JsonConverter(typeof(ValorMonetarioFlexivelConverter))]
    decimal Valor,
    string? Estabelecimento = null,
    DateOnly? Data = null,
    string? Cartao = null,
    FormaPagamento? FormaPagamento = null);
