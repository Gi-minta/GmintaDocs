using GmintaDocs.CQRS;
using GmintaDocs.Workflow.Application;
using GmintaDocs.Api.Seguridad;
using GmintaDocs.SharedKernel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GmintaDocs.Api.Controllers;

[Authorize(Policy = PoliticasDeAutorizacion.Gestion)]
[ApiController]
[Route("api/procesos")]
public sealed class ProcesosController : ControllerBase
{
    private readonly IDespachador _despachador;

    public ProcesosController(IDespachador despachador) => _despachador = despachador;

    private string Usuario => User?.Identity?.Name ?? "api";
    private string Host => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocido";

    public sealed record CrearProcesoRequest(string Nombre, string? Descripcion, long IdFormulario, string? Version);
    public sealed record ActualizarProcesoRequest(string Nombre, string? Descripcion);
    public sealed record AgregarPasoRequest(int Numero, string Descripcion, string? Prioridad, int Plazo, string? UnidadPlazo);
    public sealed record IniciarWorkflowRequest(long IdFormulario, long IdRegistro);

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<ProcesoDto>>> Listar(
        [FromQuery] int pagina = 1, [FromQuery] int tamano = 20, CancellationToken ct = default)
        => Ok(await _despachador.ConsultarAsync(new ListarProcesos(new ParametrosDePaginacion(pagina, tamano)), ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProcesoDto>> ObtenerPorId(int id, CancellationToken ct)
    {
        var proceso = await _despachador.ConsultarAsync(new ObtenerProcesoPorId(id), ct);
        return proceso is null ? NotFound() : Ok(proceso);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearProcesoRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(
            new CrearProceso(req.Nombre, req.Descripcion, req.IdFormulario, req.Version, Usuario, Host), ct);

        return resultado.EsExitoso
            ? CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Valor }, new { id = resultado.Valor })
            : BadRequest(new { error = resultado.Error });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarProcesoRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(new ActualizarProceso(id, req.Nombre, req.Descripcion), ct);
        return resultado.EsExitoso ? NoContent() : BadRequest(new { error = resultado.Error });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(new EliminarProceso(id), ct);
        return resultado.EsExitoso ? NoContent() : BadRequest(new { error = resultado.Error });
    }

    [HttpGet("{id:int}/pasos")]
    public async Task<ActionResult<IReadOnlyList<PasoDto>>> ListarPasos(int id, CancellationToken ct)
        => Ok(await _despachador.ConsultarAsync(new ListarPasosDeProceso(id), ct));

    [HttpPost("{id:int}/pasos")]
    public async Task<IActionResult> AgregarPaso(int id, [FromBody] AgregarPasoRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(
            new AgregarPaso(id, req.Numero, req.Descripcion, req.Prioridad, req.Plazo, req.UnidadPlazo, Usuario, Host), ct);

        return resultado.EsExitoso
            ? Created($"api/procesos/{id}/pasos/{resultado.Valor}", new { id = resultado.Valor })
            : BadRequest(new { error = resultado.Error });
    }

    [HttpPost("{id:int}/workflows")]
    public async Task<IActionResult> IniciarWorkflow(int id, [FromBody] IniciarWorkflowRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(
            new IniciarWorkflow(id, req.IdFormulario, req.IdRegistro, Usuario, Host), ct);

        return resultado.EsExitoso
            ? Created($"api/workflows/{resultado.Valor}", new { id = resultado.Valor })
            : BadRequest(new { error = resultado.Error });
    }

    [HttpGet("/api/workflows/{idWorkflow:long}")]
    public async Task<ActionResult<WorkflowDto>> ObtenerWorkflow(long idWorkflow, CancellationToken ct)
    {
        var workflow = await _despachador.ConsultarAsync(new ObtenerWorkflowPorId(idWorkflow), ct);
        return workflow is null ? NotFound() : Ok(workflow);
    }
}
