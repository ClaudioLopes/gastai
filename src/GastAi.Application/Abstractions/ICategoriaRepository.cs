using GastAi.Domain.Entities;

namespace GastAi.Application.Abstractions;

public interface ICategoriaRepository
{
    Task<Categoria?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Categoria>> GetAllAsync(bool incluirInativas = false, CancellationToken ct = default);
    Task<bool> ExisteComNomeAsync(string nome, Domain.Enums.TipoLancamento tipo, CancellationToken ct = default);
    Task AddAsync(Categoria categoria, CancellationToken ct = default);
    void Remove(Categoria categoria);
}
