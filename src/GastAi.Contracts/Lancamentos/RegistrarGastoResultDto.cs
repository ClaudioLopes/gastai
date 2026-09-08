namespace GastAi.Contracts.Lancamentos;

public record RegistrarGastoResultDto(LancamentoDto Lancamento, IReadOnlyList<string> AlertasOrcamento);
