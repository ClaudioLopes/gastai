using System.Reflection;
using GastAi.Domain.Entities;

namespace GastAi.Application.Tests.TestUtils;

/// <summary>
/// A navegação Categoria em Lancamento só é populada pelo EF Core via Include em
/// produção; em memória usamos reflection para simular o mesmo efeito nos testes.
/// </summary>
public static class EntidadeTestHelpers
{
    public static void AnexarCategoria(Lancamento lancamento, Categoria categoria)
    {
        typeof(Lancamento).GetProperty(nameof(Lancamento.Categoria), BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(lancamento, categoria);
    }
}
