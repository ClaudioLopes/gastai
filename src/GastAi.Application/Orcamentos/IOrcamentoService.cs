using GastAi.Contracts.Orcamentos;

namespace GastAi.Application.Orcamentos;

public interface IOrcamentoService
{
    Task<List<OrcamentoMensalDto>> ListarAsync(int ano, int mes, CancellationToken ct = default);
    Task<OrcamentoMensalDto> DefinirAsync(DefinirOrcamentoRequest request, CancellationToken ct = default);
    Task ExcluirAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Compara o total já gasto no mês (na categoria informada e no geral) contra os
    /// orçamentos configurados e retorna uma mensagem para cada limite ultrapassado.
    /// </summary>
    Task<List<string>> VerificarAlertasAsync(int categoriaId, string categoriaNome, int ano, int mes, CancellationToken ct = default);
}
