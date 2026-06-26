using GmintaDocs.AdminFormularios.Application;
using GmintaDocs.CQRS;
using GmintaDocs.Api.Seguridad;
using GmintaDocs.SharedKernel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GmintaDocs.Api.Controllers;

[Authorize(Policy = PoliticasDeAutorizacion.Gestion)]
[ApiController]
[Route("api/formularios")]
public sealed class FormulariosController : ControllerBase
{
    private readonly IDespachador _despachador;

    public FormulariosController(IDespachador despachador) => _despachador = despachador;

    private string Usuario => User?.Identity?.Name ?? "api";
    private string Host => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocido";

    public sealed record CrearFormularioRequest(string Codigo, string Tabla, string Nombre);
    public sealed record ActualizarFormularioRequest(string Nombre, string? Descripcion);
    public sealed record AgregarCampoRequest(int Orden, string Nombre, string Columna,
        short TipoDato, int LongDato, short Control, bool Requerido);

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<FormularioDto>>> Listar(
        [FromQuery] int pagina = 1, [FromQuery] int tamano = 20, CancellationToken ct = default)
        => Ok(await _despachador.ConsultarAsync(new ListarFormularios(new ParametrosDePaginacion(pagina, tamano)), ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<FormularioDto>> ObtenerPorId(long id, CancellationToken ct)
    {
        var formulario = await _despachador.ConsultarAsync(new ObtenerFormularioPorId(id), ct);
        return formulario is null ? NotFound() : Ok(formulario);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearFormularioRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(
            new CrearFormulario(req.Codigo, req.Tabla, req.Nombre, Usuario, Host), ct);

        return resultado.EsExitoso
            ? CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Valor }, new { id = resultado.Valor })
            : BadRequest(new { error = resultado.Error });
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Actualizar(long id, [FromBody] ActualizarFormularioRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(new ActualizarFormulario(id, req.Nombre, req.Descripcion), ct);
        return resultado.EsExitoso ? NoContent() : BadRequest(new { error = resultado.Error });
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Eliminar(long id, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(new EliminarFormulario(id), ct);
        return resultado.EsExitoso ? NoContent() : BadRequest(new { error = resultado.Error });
    }

    [HttpGet("{id:long}/campos")]
    public async Task<ActionResult<IReadOnlyList<CampoDto>>> ListarCampos(long id, CancellationToken ct)
        => Ok(await _despachador.ConsultarAsync(new ListarCamposDeFormulario(id), ct));

    [HttpPost("{id:long}/campos")]
    public async Task<IActionResult> AgregarCampo(long id, [FromBody] AgregarCampoRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(
            new AgregarCampo(id, req.Orden, req.Nombre, req.Columna, req.TipoDato, req.LongDato,
                req.Control, req.Requerido, Usuario, Host), ct);

        return resultado.EsExitoso
            ? Created($"api/formularios/{id}/campos/{resultado.Valor}", new { id = resultado.Valor })
            : BadRequest(new { error = resultado.Error });
    }
}
