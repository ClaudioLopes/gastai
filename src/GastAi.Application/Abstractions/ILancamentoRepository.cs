using GastAi.Domain.Entities;

namespace GastAi.Application.Abstractions;

public interface ILancamentoRepository
{
    Task<Lancamento?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Lançamentos cuja Data cai dentro do mês informado (inclusive nas duas pontas), com Categoria carregada.</summary>
    Task<List<Lancamento>> GetByMesAsync(int ano, int mes, CancellationToken ct = default);

    /// <summary>Lançamentos com Data de <paramref name="inicio"/> (inclusive) até <paramref name="fim"/> (exclusive), com Categoria carregada.</summary>
    Task<List<Lancamento>> GetByPeriodoAsync(DateOnly inicio, DateOnly fim, CancellationToken ct = default);

    /// <summary>Todos os lançamentos ainda marcados como "a confirmar", de qualquer mês, com Categoria carregada.</summary>
    Task<List<Lancamento>> GetAConfirmarAsync(CancellationToken ct = default);

    /// <summary>Categoria do gasto mais recente com exatamente esta descrição, ou null se nunca houve.</summary>
    Task<int?> GetCategoriaDaUltimaCompraAsync(string descricao, CancellationToken ct = default);

    Task AddAsync(Lancamento lancamento, CancellationToken ct = default);
    void Remove(Lancamento lancamento);
}
