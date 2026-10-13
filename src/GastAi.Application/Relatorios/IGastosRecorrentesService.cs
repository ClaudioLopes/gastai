using GastAi.Contracts.Relatorios;

namespace GastAi.Application.Relatorios;

public interface IGastosRecorrentesService
{
    /// <summary>
    /// Gastos que se repetem em pelo menos dois dos últimos <paramref name="meses"/> meses
    /// (contando o mês informado), com a evolução do valor mês a mês.
    /// </summary>
    Task<IReadOnlyList<GastoRecorrenteDto>> ListarAsync(int ano, int mes, int meses = 6, CancellationToken ct = default);
}
