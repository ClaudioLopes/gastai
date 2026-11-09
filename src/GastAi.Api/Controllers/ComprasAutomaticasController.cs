using System.Security.Cryptography;
using System.Text;
using GastAi.Application.Lancamentos;
using GastAi.Contracts.Lancamentos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GastAi.Api.Controllers;

/// <summary>
/// Entrada para automações externas (ex.: Atalhos do iPhone ao pagar com Apple Pay). Não usa o
/// cookie de login: a automação se identifica com a chave do cabeçalho "X-Api-Key", configurada
/// em "Integracoes:ChaveApi". Sem chave configurada, o endpoint fica desligado.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/integracoes/compras")]
public class ComprasAutomaticasController(
    ICompraAutomaticaService compraAutomaticaService,
    IConfiguration configuration) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<LancamentoDto>> Registrar(
        [FromHeader(Name = "X-Api-Key")] string? chave,
        CompraAutomaticaRequest request,
        CancellationToken ct)
    {
        if (!ChaveValida(chave))
            return Unauthorized();

        return Ok(await compraAutomaticaService.RegistrarAsync(request, ct));
    }

    private bool ChaveValida(string? informada)
    {
        var esperada = configuration["Integracoes:ChaveApi"];

        if (string.IsNullOrWhiteSpace(esperada) || string.IsNullOrEmpty(informada))
            return false;

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(informada),
            Encoding.UTF8.GetBytes(esperada));
    }
}
