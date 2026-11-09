using GastAi.Contracts.Lancamentos;

namespace GastAi.Application.Lancamentos;

public interface ICompraAutomaticaService
{
    /// <summary>
    /// Registra uma compra vinda de uma automação como gasto "a confirmar". A categoria é a mesma
    /// da última compra com o mesmo estabelecimento; sem histórico, cai em "Outros".
    /// </summary>
    Task<LancamentoDto> RegistrarAsync(CompraAutomaticaRequest request, CancellationToken ct = default);
}
