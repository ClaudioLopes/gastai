using GastAi.Contracts.Lancamentos;

namespace GastAi.Application.Lancamentos;

public interface ILancamentoService
{
    Task<RegistrarGastoResultDto> RegistrarGastoAsync(CriarGastoRequest request, CancellationToken ct = default);
    Task<LancamentoDto> RegistrarReceitaAsync(CriarReceitaRequest request, CancellationToken ct = default);
    Task<LancamentoDto> AtualizarAsync(int id, AtualizarLancamentoRequest request, CancellationToken ct = default);
    Task ExcluirAsync(int id, CancellationToken ct = default);
    Task<List<LancamentoDto>> ListarPorMesAsync(int ano, int mes, CancellationToken ct = default);

    /// <summary>Compras registradas automaticamente que o usuário ainda não revisou (de qualquer mês).</summary>
    Task<List<LancamentoDto>> ListarAConfirmarAsync(CancellationToken ct = default);

    Task<LancamentoDto> ConfirmarAsync(int id, CancellationToken ct = default);
}
