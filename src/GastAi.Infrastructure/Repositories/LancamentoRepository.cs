using GastAi.Application.Abstractions;
using GastAi.Domain.Entities;
using GastAi.Domain.Enums;
using GastAi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GastAi.Infrastructure.Repositories;

public class LancamentoRepository(GastAiDbContext db) : ILancamentoRepository
{
    public Task<Lancamento?> GetByIdAsync(int id, CancellationToken ct = default)
        => db.Lancamentos.Include(l => l.Categoria).FirstOrDefaultAsync(l => l.Id == id, ct);

    public Task<List<Lancamento>> GetByMesAsync(int ano, int mes, CancellationToken ct = default)
    {
        var inicio = new DateOnly(ano, mes, 1);
        return GetByPeriodoAsync(inicio, inicio.AddMonths(1), ct);
    }

    public Task<List<Lancamento>> GetByPeriodoAsync(DateOnly inicio, DateOnly fim, CancellationToken ct = default)
        => db.Lancamentos
            .Include(l => l.Categoria)
            .Where(l => l.Data >= inicio && l.Data < fim)
            .ToListAsync(ct);

    public Task<List<Lancamento>> GetAConfirmarAsync(CancellationToken ct = default)
        => db.Lancamentos
            .Include(l => l.Categoria)
            .Where(l => l.AConfirmar)
            .ToListAsync(ct);

    public async Task<int?> GetCategoriaDaUltimaCompraAsync(string descricao, CancellationToken ct = default)
    {
        // Só considera lançamentos já revisados: uma compra automática ainda não confirmada
        // pode estar com a categoria padrão e não deve "ensinar" a categoria errada.
        var categoriaId = await db.Lancamentos
            .Where(l => l.Tipo == TipoLancamento.Gasto && !l.AConfirmar && l.Descricao == descricao)
            .OrderByDescending(l => l.Data)
            .ThenByDescending(l => l.Id)
            .Select(l => l.CategoriaId)
            .FirstOrDefaultAsync(ct);

        return categoriaId == 0 ? null : categoriaId;
    }

    public async Task AddAsync(Lancamento lancamento, CancellationToken ct = default)
        => await db.Lancamentos.AddAsync(lancamento, ct);

    public void Remove(Lancamento lancamento)
        => db.Lancamentos.Remove(lancamento);
}
