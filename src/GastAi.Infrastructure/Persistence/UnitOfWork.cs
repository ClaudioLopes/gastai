using GastAi.Application.Abstractions;

namespace GastAi.Infrastructure.Persistence;

public class UnitOfWork(GastAiDbContext db) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
