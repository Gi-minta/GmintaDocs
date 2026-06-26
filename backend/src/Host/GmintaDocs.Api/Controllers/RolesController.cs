using GmintaDocs.Api.Seguridad;
using GmintaDocs.CQRS;
using GmintaDocs.Identidad.Application;
using GmintaDocs.SharedKernel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GmintaDocs.Api.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Roles = RolesDelSistema.Administrador)]
public sealed class RolesController : ControllerBase
{
    private readonly IDespachador _despachador;

    public RolesController(IDespachador despachador) => _despachador = despachador;

    public sealed record CrearRolRequest(string Nombre);

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<RolDto>>> Listar(
        [FromQuery] int pagina = 1, [FromQuery] int tamano = 20, CancellationToken ct = default)
        => Ok(await _despachador.ConsultarAsync(new ListarRoles(new ParametrosDePaginacion(pagina, tamano)), ct));

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearRolRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(new CrearRol(req.Nombre), ct);
        return resultado.EsExitoso
            ? Created($"api/roles/{resultado.Valor}", new { id = resultado.Valor })
            : BadRequest(new { error = resultado.Error });
    }
}
