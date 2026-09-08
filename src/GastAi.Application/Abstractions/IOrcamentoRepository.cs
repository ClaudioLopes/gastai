using GastAi.Domain.Entities;

namespace GastAi.Application.Abstractions;

public interface IOrcamentoRepository
{
    Task<OrcamentoMensal?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>categoriaId nulo busca o orçamento geral do mês.</summary>
    Task<OrcamentoMensal?> GetByCategoriaEMesAsync(int? categoriaId, DateOnly mesReferencia, CancellationToken ct = default);

    /// <summary>Todos os orçamentos (geral + por categoria) definidos para o mês, com Categoria carregada.</summary>
    Task<List<OrcamentoMensal>> GetByMesAsync(DateOnly mesReferencia, CancellationToken ct = default);

    Task AddAsync(OrcamentoMensal orcamento, CancellationToken ct = default);
    void Remove(OrcamentoMensal orcamento);
}
