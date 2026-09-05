using GastAi.Contracts.Relatorios;

namespace GastAi.Application.Relatorios;

public interface IRelatorioMensalService
{
    Task<RelatorioMensalDto> GerarAsync(int ano, int mes, CancellationToken ct = default);
}
