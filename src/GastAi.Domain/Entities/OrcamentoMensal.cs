using GastAi.Domain.Exceptions;

namespace GastAi.Domain.Entities;

/// <summary>
/// Modelado desde a Fase 1 para não exigir retrabalho quando a Fase 2
/// (orçamento/meta mensal) for implementada — sem UI/API própria ainda.
/// </summary>
public class OrcamentoMensal
{
    public int Id { get; private set; }
    public int? CategoriaId { get; private set; }
    public Categoria? Categoria { get; private set; }

    /// <summary>Primeiro dia do mês de referência (ano-mês).</summary>
    public DateOnly MesReferencia { get; private set; }
    public decimal ValorLimite { get; private set; }

    private OrcamentoMensal()
    {
    }

    public OrcamentoMensal(int? categoriaId, DateOnly mesReferencia, decimal valorLimite)
    {
        if (valorLimite < 0)
            throw new DomainException("O limite de orçamento não pode ser negativo.");

        CategoriaId = categoriaId;
        MesReferencia = new DateOnly(mesReferencia.Year, mesReferencia.Month, 1);
        ValorLimite = valorLimite;
    }

    public void AtualizarLimite(decimal valorLimite)
    {
        if (valorLimite < 0)
            throw new DomainException("O limite de orçamento não pode ser negativo.");

        ValorLimite = valorLimite;
    }
}
