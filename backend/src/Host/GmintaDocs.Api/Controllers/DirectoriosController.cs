using GmintaDocs.AdminDirectorios.Application;
using GmintaDocs.CQRS;
using GmintaDocs.Api.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GmintaDocs.Api.Controllers;

[Authorize(Policy = PoliticasDeAutorizacion.Gestion)]
[ApiController]
[Route("api/directorios")]
public sealed class DirectoriosController : ControllerBase
{
    private readonly IDespachador _despachador;

    public DirectoriosController(IDespachador despachador) => _despachador = despachador;

    private string Usuario => User?.Identity?.Name ?? "api";
    private string Host => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocido";

    public sealed record CrearDirectorioRequest(long IdFormulario, long IdDirectorioPadre, string Codigo, string Nombre);
    public sealed record ActualizarDirectorioRequest(string Nombre, string Codigo);

    [HttpGet("{id:long}")]
    public async Task<ActionResult<DirectorioDto>> ObtenerPorId(long id, CancellationToken ct)
    {
        var directorio = await _despachador.ConsultarAsync(new ObtenerDirectorioPorId(id), ct);
        return directorio is null ? NotFound() : Ok(directorio);
    }

    [HttpGet("formulario/{idFormulario:long}")]
    public async Task<ActionResult<IReadOnlyList<DirectorioDto>>> ListarPorFormulario(long idFormulario, CancellationToken ct)
        => Ok(await _despachador.ConsultarAsync(new ListarDirectoriosDeFormulario(idFormulario), ct));

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearDirectorioRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(
            new CrearDirectorio(req.IdFormulario, req.IdDirectorioPadre, req.Codigo, req.Nombre, Usuario, Host), ct);

        return resultado.EsExitoso
            ? CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Valor }, new { id = resultado.Valor })
            : BadRequest(new { error = resultado.Error });
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Actualizar(long id, [FromBody] ActualizarDirectorioRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(new ActualizarDirectorio(id, req.Nombre, req.Codigo), ct);
        return resultado.EsExitoso ? NoContent() : BadRequest(new { error = resultado.Error });
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Eliminar(long id, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(new EliminarDirectorio(id), ct);
        return resultado.EsExitoso ? NoContent() : BadRequest(new { error = resultado.Error });
    }
}
