using GastAi.Application.Abstractions;
using GastAi.Domain.Entities;
using GastAi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GastAi.Infrastructure.Repositories;

public class OrcamentoRepository(GastAiDbContext db) : IOrcamentoRepository
{
    public Task<OrcamentoMensal?> GetByIdAsync(int id, CancellationToken ct = default)
        => db.OrcamentosMensais.FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<OrcamentoMensal?> GetByCategoriaEMesAsync(int? categoriaId, DateOnly mesReferencia, CancellationToken ct = default)
        => db.OrcamentosMensais.FirstOrDefaultAsync(
            o => o.CategoriaId == categoriaId && o.MesReferencia == mesReferencia, ct);

    public Task<List<OrcamentoMensal>> GetByMesAsync(DateOnly mesReferencia, CancellationToken ct = default)
        => db.OrcamentosMensais
            .Include(o => o.Categoria)
            .Where(o => o.MesReferencia == mesReferencia)
            .ToListAsync(ct);

    public async Task AddAsync(OrcamentoMensal orcamento, CancellationToken ct = default)
        => await db.OrcamentosMensais.AddAsync(orcamento, ct);

    public void Remove(OrcamentoMensal orcamento)
        => db.OrcamentosMensais.Remove(orcamento);
}
