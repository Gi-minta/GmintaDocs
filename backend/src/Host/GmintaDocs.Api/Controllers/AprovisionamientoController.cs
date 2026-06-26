using GmintaDocs.Api.Persistencia;
using GmintaDocs.Api.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GmintaDocs.Api.Controllers;

/// <summary>
/// Operaciones de aprovisionamiento de tenants: crea y migra la base de datos dedicada de una empresa.
/// Endpoint de administración del núcleo (requiere rol Administrador).
/// </summary>
[ApiController]
[Route("api/empresas")]
[Authorize(Roles = RolesDelSistema.Administrador)]
public sealed class AprovisionamientoController : ControllerBase
{
    private readonly AprovisionadorDeEmpresa _aprovisionador;

    public AprovisionamientoController(AprovisionadorDeEmpresa aprovisionador) => _aprovisionador = aprovisionador;

    [HttpPost("{idEmpresa:long}/aprovisionar")]
    public async Task<IActionResult> Aprovisionar(long idEmpresa, CancellationToken ct)
    {
        if (idEmpresa <= 0)
            return BadRequest(new { error = "El identificador de empresa no es válido." });

        try
        {
            await _aprovisionador.AprovisionarAsync(idEmpresa, ct);
            return Ok(new { idEmpresa, estado = "aprovisionada" });
        }
        catch (Exception ex)
        {
            return Problem(title: "No se pudo aprovisionar la empresa.", detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
