using GastAi.Application.Relatorios;
using GastAi.Contracts.Relatorios;
using Microsoft.AspNetCore.Mvc;

namespace GastAi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RelatoriosController(
    IRelatorioMensalService relatorioMensalService,
    IGastosRecorrentesService gastosRecorrentesService) : ControllerBase
{
    [HttpGet("recorrentes")]
    public async Task<ActionResult<IReadOnlyList<GastoRecorrenteDto>>> Recorrentes(
        [FromQuery] int ano, [FromQuery] int mes, [FromQuery] int meses = 6, CancellationToken ct = default)
        => Ok(await gastosRecorrentesService.ListarAsync(ano, mes, meses, ct));

    [HttpGet("mensal")]
    public async Task<ActionResult<RelatorioMensalDto>> Mensal(
        [FromQuery] int ano, [FromQuery] int mes, CancellationToken ct)
        => Ok(await relatorioMensalService.GerarAsync(ano, mes, ct));
}
