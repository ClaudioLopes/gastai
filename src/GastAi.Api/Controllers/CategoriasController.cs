using GastAi.Application.Categorias;
using GastAi.Contracts.Categorias;
using GastAi.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace GastAi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController(ICategoriaService categoriaService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CategoriaDto>>> Listar(
        [FromQuery] TipoLancamento? tipo,
        [FromQuery] bool incluirInativas = false,
        CancellationToken ct = default)
        => Ok(await categoriaService.ListarAsync(tipo, incluirInativas, ct));

    [HttpPost]
    public async Task<ActionResult<CategoriaDto>> Criar(CriarCategoriaRequest request, CancellationToken ct)
    {
        var categoria = await categoriaService.CriarAsync(request, ct);
        return Ok(categoria);
    }

    [HttpPost("{id:int}/desativar")]
    public async Task<IActionResult> Desativar(int id, CancellationToken ct)
    {
        await categoriaService.DesativarAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/reativar")]
    public async Task<IActionResult> Reativar(int id, CancellationToken ct)
    {
        await categoriaService.ReativarAsync(id, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        await categoriaService.ExcluirAsync(id, ct);
        return NoContent();
    }
}
