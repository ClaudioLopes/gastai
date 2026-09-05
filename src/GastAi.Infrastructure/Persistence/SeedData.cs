using GastAi.Domain.Enums;

namespace GastAi.Infrastructure.Persistence;

/// <summary>
/// Dados de seed para <c>HasData</c>. Usa objetos anônimos (em vez de instanciar
/// <see cref="GastAi.Domain.Entities.Categoria"/>) porque a entidade só expõe
/// setters privados — é o padrão recomendado do EF Core para esse caso.
/// </summary>
public static class SeedData
{
    public static readonly object[] Categorias =
    [
        new { Id = 1, Nome = "Alimentação", Tipo = TipoLancamento.Gasto, Cor = (string?)"#F97316", PadraoDoSistema = true, Ativa = true },
        new { Id = 2, Nome = "Transporte", Tipo = TipoLancamento.Gasto, Cor = (string?)"#3B82F6", PadraoDoSistema = true, Ativa = true },
        new { Id = 3, Nome = "Moradia", Tipo = TipoLancamento.Gasto, Cor = (string?)"#8B5CF6", PadraoDoSistema = true, Ativa = true },
        new { Id = 4, Nome = "Lazer", Tipo = TipoLancamento.Gasto, Cor = (string?)"#EC4899", PadraoDoSistema = true, Ativa = true },
        new { Id = 5, Nome = "Saúde", Tipo = TipoLancamento.Gasto, Cor = (string?)"#EF4444", PadraoDoSistema = true, Ativa = true },
        new { Id = 6, Nome = "Outros", Tipo = TipoLancamento.Gasto, Cor = (string?)"#6B7280", PadraoDoSistema = true, Ativa = true },
        new { Id = 7, Nome = "Salário", Tipo = TipoLancamento.Receita, Cor = (string?)"#22C55E", PadraoDoSistema = true, Ativa = true },
        new { Id = 8, Nome = "Freelance", Tipo = TipoLancamento.Receita, Cor = (string?)"#14B8A6", PadraoDoSistema = true, Ativa = true },
        new { Id = 9, Nome = "Outro", Tipo = TipoLancamento.Receita, Cor = (string?)"#6B7280", PadraoDoSistema = true, Ativa = true },
    ];
}
