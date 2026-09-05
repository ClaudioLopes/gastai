using GastAi.Domain.Entities;

namespace GastAi.Application.Abstractions;

public interface ILancamentoRepository
{
    Task<Lancamento?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Lançamentos cuja Data cai dentro do mês informado (inclusive nas duas pontas), com Categoria carregada.</summary>
    Task<List<Lancamento>> GetByMesAsync(int ano, int mes, CancellationToken ct = default);

    Task AddAsync(Lancamento lancamento, CancellationToken ct = default);
    void Remove(Lancamento lancamento);
}
