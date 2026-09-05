using GastAi.Contracts.Lancamentos;

namespace GastAi.Application.Lancamentos;

public interface ILancamentoService
{
    Task<LancamentoDto> RegistrarGastoAsync(CriarGastoRequest request, CancellationToken ct = default);
    Task<LancamentoDto> RegistrarReceitaAsync(CriarReceitaRequest request, CancellationToken ct = default);
    Task<LancamentoDto> AtualizarAsync(int id, AtualizarLancamentoRequest request, CancellationToken ct = default);
    Task ExcluirAsync(int id, CancellationToken ct = default);
    Task<List<LancamentoDto>> ListarPorMesAsync(int ano, int mes, CancellationToken ct = default);
}
