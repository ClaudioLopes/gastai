using GastAi.Application.Relatorios;
using GastAi.Contracts.Relatorios;
using Microsoft.AspNetCore.Mvc;

namespace GastAi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RelatoriosController(IRelatorioMensalService relatorioMensalService) : ControllerBase
{
    [HttpGet("mensal")]
    public async Task<ActionResult<RelatorioMensalDto>> Mensal(
        [FromQuery] int ano, [FromQuery] int mes, CancellationToken ct)
        => Ok(await relatorioMensalService.GerarAsync(ano, mes, ct));
}
