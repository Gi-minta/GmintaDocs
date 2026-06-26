using GmintaDocs.Api.Seguridad;
using GmintaDocs.CQRS;
using GmintaDocs.Identidad.Application;
using GmintaDocs.SharedKernel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GmintaDocs.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = RolesDelSistema.Administrador)]
public sealed class UsuariosController : ControllerBase
{
    private readonly IDespachador _despachador;

    public UsuariosController(IDespachador despachador) => _despachador = despachador;

    public sealed record CrearUsuarioRequest(string UserName, string? FullName, string? Email, string? Contrasena);
    public sealed record AsignarRolRequest(string IdRol);

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<UsuarioDto>>> Listar(
        [FromQuery] int pagina = 1, [FromQuery] int tamano = 20, CancellationToken ct = default)
        => Ok(await _despachador.ConsultarAsync(new ListarUsuarios(new ParametrosDePaginacion(pagina, tamano)), ct));

    [HttpGet("{id}")]
    public async Task<ActionResult<UsuarioDto>> ObtenerPorId(string id, CancellationToken ct)
    {
        var usuario = await _despachador.ConsultarAsync(new ObtenerUsuarioPorId(id), ct);
        return usuario is null ? NotFound() : Ok(usuario);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearUsuarioRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(
            new CrearUsuario(req.UserName, req.FullName, req.Email, req.Contrasena), ct);

        return resultado.EsExitoso
            ? CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Valor }, new { id = resultado.Valor })
            : BadRequest(new { error = resultado.Error });
    }

    [HttpPost("{id}/roles")]
    public async Task<IActionResult> AsignarRol(string id, [FromBody] AsignarRolRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(new AsignarRolAUsuario(id, req.IdRol), ct);
        return resultado.EsExitoso ? NoContent() : BadRequest(new { error = resultado.Error });
    }

    [HttpPost("{id}/desactivar")]
    public async Task<IActionResult> Desactivar(string id, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(new DesactivarUsuario(id), ct);
        return resultado.EsExitoso ? NoContent() : BadRequest(new { error = resultado.Error });
    }
}
