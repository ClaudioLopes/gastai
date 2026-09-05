using GastAi.Application.Abstractions;
using GastAi.Domain.Entities;
using GastAi.Domain.Enums;
using GastAi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GastAi.Infrastructure.Repositories;

public class CategoriaRepository(GastAiDbContext db) : ICategoriaRepository
{
    public Task<Categoria?> GetByIdAsync(int id, CancellationToken ct = default)
        => db.Categorias.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<List<Categoria>> GetAllAsync(bool incluirInativas = false, CancellationToken ct = default)
    {
        var query = db.Categorias.AsQueryable();

        if (!incluirInativas)
            query = query.Where(c => c.Ativa);

        return await query.ToListAsync(ct);
    }

    public Task<bool> ExisteComNomeAsync(string nome, TipoLancamento tipo, CancellationToken ct = default)
        => db.Categorias.AnyAsync(c => c.Tipo == tipo && c.Nome.ToLower() == nome.Trim().ToLower(), ct);

    public async Task AddAsync(Categoria categoria, CancellationToken ct = default)
        => await db.Categorias.AddAsync(categoria, ct);

    public void Remove(Categoria categoria)
        => db.Categorias.Remove(categoria);
}
