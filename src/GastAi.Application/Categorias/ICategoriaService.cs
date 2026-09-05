using GastAi.Contracts.Categorias;
using GastAi.Domain.Enums;

namespace GastAi.Application.Categorias;

public interface ICategoriaService
{
    Task<List<CategoriaDto>> ListarAsync(TipoLancamento? tipo, bool incluirInativas, CancellationToken ct = default);
    Task<CategoriaDto> CriarAsync(CriarCategoriaRequest request, CancellationToken ct = default);
    Task DesativarAsync(int id, CancellationToken ct = default);
    Task ReativarAsync(int id, CancellationToken ct = default);
    Task ExcluirAsync(int id, CancellationToken ct = default);
}
