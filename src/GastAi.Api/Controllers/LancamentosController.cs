using GastAi.Application.Lancamentos;
using GastAi.Contracts.Lancamentos;
using Microsoft.AspNetCore.Mvc;

namespace GastAi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LancamentosController(ILancamentoService lancamentoService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<LancamentoDto>>> ListarPorMes(
        [FromQuery] int ano, [FromQuery] int mes, CancellationToken ct)
        => Ok(await lancamentoService.ListarPorMesAsync(ano, mes, ct));

    [HttpPost("gastos")]
    public async Task<ActionResult<LancamentoDto>> RegistrarGasto(CriarGastoRequest request, CancellationToken ct)
        => Ok(await lancamentoService.RegistrarGastoAsync(request, ct));

    [HttpPost("receitas")]
    public async Task<ActionResult<LancamentoDto>> RegistrarReceita(CriarReceitaRequest request, CancellationToken ct)
        => Ok(await lancamentoService.RegistrarReceitaAsync(request, ct));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<LancamentoDto>> Atualizar(int id, AtualizarLancamentoRequest request, CancellationToken ct)
        => Ok(await lancamentoService.AtualizarAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        await lancamentoService.ExcluirAsync(id, ct);
        return NoContent();
    }
}
