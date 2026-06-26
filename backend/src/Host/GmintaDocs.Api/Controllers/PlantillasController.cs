using GmintaDocs.CQRS;
using GmintaDocs.Plantillas.Application;
using GmintaDocs.Api.Seguridad;
using GmintaDocs.SharedKernel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GmintaDocs.Api.Controllers;

[Authorize(Policy = PoliticasDeAutorizacion.Gestion)]
[ApiController]
public sealed class PlantillasController : ControllerBase
{
    private readonly IDespachador _despachador;

    public PlantillasController(IDespachador despachador) => _despachador = despachador;

    private string Usuario => User?.Identity?.Name ?? "api";
    private string Host => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocido";

    public sealed record CrearPlantillaRequest(string Codigo, string Nombre, string? Contenido);
    public sealed record ActualizarPlantillaRequest(string Nombre, string? Contenido);
    public sealed record CrearPlantillaFormatoRequest(string Codigo, string Nombre, string? FormatoHtml, long IdFormulario);

    [HttpGet("api/plantillas")]
    public async Task<ActionResult<ResultadoPaginado<PlantillaDto>>> Listar(
        [FromQuery] int pagina = 1, [FromQuery] int tamano = 20, CancellationToken ct = default)
        => Ok(await _despachador.ConsultarAsync(new ListarPlantillas(new ParametrosDePaginacion(pagina, tamano)), ct));

    [HttpGet("api/plantillas/{id:int}")]
    public async Task<ActionResult<PlantillaDto>> ObtenerPorId(int id, CancellationToken ct)
    {
        var plantilla = await _despachador.ConsultarAsync(new ObtenerPlantillaPorId(id), ct);
        return plantilla is null ? NotFound() : Ok(plantilla);
    }

    [HttpPost("api/plantillas")]
    public async Task<IActionResult> Crear([FromBody] CrearPlantillaRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(new CrearPlantilla(req.Codigo, req.Nombre, req.Contenido), ct);
        return resultado.EsExitoso
            ? Created($"api/plantillas/{resultado.Valor}", new { id = resultado.Valor })
            : BadRequest(new { error = resultado.Error });
    }

    [HttpPut("api/plantillas/{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarPlantillaRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(new ActualizarPlantilla(id, req.Nombre, req.Contenido), ct);
        return resultado.EsExitoso ? NoContent() : BadRequest(new { error = resultado.Error });
    }

    [HttpDelete("api/plantillas/{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(new EliminarPlantilla(id), ct);
        return resultado.EsExitoso ? NoContent() : BadRequest(new { error = resultado.Error });
    }

    [HttpGet("api/plantillas-formato")]
    public async Task<ActionResult<IReadOnlyList<PlantillaFormatoDto>>> ListarFormatos(CancellationToken ct)
        => Ok(await _despachador.ConsultarAsync(new ListarPlantillasFormato(), ct));

    [HttpPost("api/plantillas-formato")]
    public async Task<IActionResult> CrearFormato([FromBody] CrearPlantillaFormatoRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(
            new CrearPlantillaFormato(req.Codigo, req.Nombre, req.FormatoHtml, req.IdFormulario, Usuario, Host), ct);

        return resultado.EsExitoso
            ? Created($"api/plantillas-formato/{resultado.Valor}", new { id = resultado.Valor })
            : BadRequest(new { error = resultado.Error });
    }
}
