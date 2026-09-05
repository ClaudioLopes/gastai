using GastAi.Application.Abstractions;
using GastAi.Domain.Entities;
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
        var fim = inicio.AddMonths(1);

        return db.Lancamentos
            .Include(l => l.Categoria)
            .Where(l => l.Data >= inicio && l.Data < fim)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Lancamento lancamento, CancellationToken ct = default)
        => await db.Lancamentos.AddAsync(lancamento, ct);

    public void Remove(Lancamento lancamento)
        => db.Lancamentos.Remove(lancamento);
}
