namespace GastAi.Contracts.Lancamentos;

/// <summary>Lancamento é a primeira parcela; TotalParcelasGeradas é 1 para um gasto não parcelado.</summary>
public record RegistrarGastoResultDto(LancamentoDto Lancamento, IReadOnlyList<string> AlertasOrcamento, int TotalParcelasGeradas = 1);
