using GastAi.Application.Orcamentos;
using GastAi.Contracts.Orcamentos;
using Microsoft.AspNetCore.Mvc;

namespace GastAi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrcamentosController(IOrcamentoService orcamentoService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<OrcamentoMensalDto>>> Listar(
        [FromQuery] int ano, [FromQuery] int mes, CancellationToken ct)
        => Ok(await orcamentoService.ListarAsync(ano, mes, ct));

    [HttpPut]
    public async Task<ActionResult<OrcamentoMensalDto>> Definir(DefinirOrcamentoRequest request, CancellationToken ct)
        => Ok(await orcamentoService.DefinirAsync(request, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        await orcamentoService.ExcluirAsync(id, ct);
        return NoContent();
    }
}
