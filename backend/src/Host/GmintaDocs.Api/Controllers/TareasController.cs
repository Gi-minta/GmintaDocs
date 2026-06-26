using GmintaDocs.CQRS;
using GmintaDocs.Tareas.Application;
using GmintaDocs.Api.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GmintaDocs.Api.Controllers;

[Authorize(Policy = PoliticasDeAutorizacion.Gestion)]
[ApiController]
[Route("api/tareas")]
public sealed class TareasController : ControllerBase
{
    private readonly IDespachador _despachador;

    public TareasController(IDespachador despachador) => _despachador = despachador;

    private string Host => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocido";

    public sealed record CrearTareaRequest(long IdWorkflow, string Asunto, string? Descripcion, string? Prioridad,
        int Paso, string Responsable, string? Remitente, DateTime FechaVencimiento);
    public sealed record EjecutarTareaRequest(short Estado);
    public sealed record ActualizarTareaRequest(string Asunto, string? Descripcion, string? Prioridad,
        string Responsable, DateTime FechaVencimiento);

    [HttpGet("{id:long}")]
    public async Task<ActionResult<TareaDto>> ObtenerPorId(long id, CancellationToken ct)
    {
        var tarea = await _despachador.ConsultarAsync(new ObtenerTareaPorId(id), ct);
        return tarea is null ? NotFound() : Ok(tarea);
    }

    [HttpGet("responsable/{responsable}")]
    public async Task<ActionResult<IReadOnlyList<TareaDto>>> ListarPorResponsable(string responsable, CancellationToken ct)
        => Ok(await _despachador.ConsultarAsync(new ListarTareasPorResponsable(responsable), ct));

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearTareaRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(
            new CrearTarea(req.IdWorkflow, req.Asunto, req.Descripcion, req.Prioridad, req.Paso,
                req.Responsable, req.Remitente, req.FechaVencimiento, Host), ct);

        return resultado.EsExitoso
            ? CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Valor }, new { id = resultado.Valor })
            : BadRequest(new { error = resultado.Error });
    }

    [HttpPost("{id:long}/ejecutar")]
    public async Task<IActionResult> Ejecutar(long id, [FromBody] EjecutarTareaRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(new EjecutarTarea(id, req.Estado), ct);
        return resultado.EsExitoso ? NoContent() : BadRequest(new { error = resultado.Error });
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Actualizar(long id, [FromBody] ActualizarTareaRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(
            new ActualizarTarea(id, req.Asunto, req.Descripcion, req.Prioridad, req.Responsable, req.FechaVencimiento), ct);
        return resultado.EsExitoso ? NoContent() : BadRequest(new { error = resultado.Error });
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Eliminar(long id, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(new EliminarTarea(id), ct);
        return resultado.EsExitoso ? NoContent() : BadRequest(new { error = resultado.Error });
    }
}
