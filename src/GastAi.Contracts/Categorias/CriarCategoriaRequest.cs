using GastAi.Domain.Enums;

namespace GastAi.Contracts.Categorias;

public record CriarCategoriaRequest(
    string Nome,
    TipoLancamento Tipo,
    string? Cor);
