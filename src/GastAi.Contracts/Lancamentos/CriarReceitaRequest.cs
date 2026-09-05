namespace GastAi.Contracts.Lancamentos;

public record CriarReceitaRequest(
    decimal Valor,
    DateOnly Data,
    string Descricao,
    int CategoriaId,
    string? Observacao);
