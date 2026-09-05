using GastAi.Domain.Enums;

namespace GastAi.Contracts.Categorias;

public record CategoriaDto(
    int Id,
    string Nome,
    TipoLancamento Tipo,
    string? Cor,
    bool PadraoDoSistema,
    bool Ativa);
