using GmintaDocs.CQRS;
using GmintaDocs.GestionDocumental.Application;
using GmintaDocs.Api.Seguridad;
using GmintaDocs.SharedKernel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GmintaDocs.Api.Controllers;

[Authorize(Policy = PoliticasDeAutorizacion.Gestion)]
[ApiController]
public sealed class DocumentosController : ControllerBase
{
    private readonly IDespachador _despachador;

    public DocumentosController(IDespachador despachador) => _despachador = despachador;

    private string Usuario => User?.Identity?.Name ?? "api";
    private string Host => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocido";

    public sealed record CrearTipoDocumentoRequest(string Codigo, string Nombre, bool ControlaVersion,
        int DiasVigencia, bool ControlaVigencia);
    public sealed record ActualizarTipoDocumentoRequest(string Nombre, bool ControlaVersion,
        int DiasVigencia, bool ControlaVigencia);
    public sealed record RegistrarArchivoRequest(string Nombre, string Extension, string Directorio,
        long IdTipoDocumento, long Bytes, string? Version, string? Descripcion);

    [HttpGet("api/tipos-documento")]
    public async Task<ActionResult<ResultadoPaginado<TipoDocumentoDto>>> ListarTipos(
        [FromQuery] int pagina = 1, [FromQuery] int tamano = 20, CancellationToken ct = default)
        => Ok(await _despachador.ConsultarAsync(new ListarTiposDocumento(new ParametrosDePaginacion(pagina, tamano)), ct));

    [HttpPost("api/tipos-documento")]
    public async Task<IActionResult> CrearTipo([FromBody] CrearTipoDocumentoRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(
            new CrearTipoDocumento(req.Codigo, req.Nombre, req.ControlaVersion, req.DiasVigencia,
                req.ControlaVigencia, Usuario, Host), ct);

        return resultado.EsExitoso
            ? Created($"api/tipos-documento/{resultado.Valor}", new { id = resultado.Valor })
            : BadRequest(new { error = resultado.Error });
    }

    [HttpPut("api/tipos-documento/{id:long}")]
    public async Task<IActionResult> ActualizarTipo(long id, [FromBody] ActualizarTipoDocumentoRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(
            new ActualizarTipoDocumento(id, req.Nombre, req.ControlaVersion, req.DiasVigencia, req.ControlaVigencia), ct);
        return resultado.EsExitoso ? NoContent() : BadRequest(new { error = resultado.Error });
    }

    [HttpDelete("api/tipos-documento/{id:long}")]
    public async Task<IActionResult> EliminarTipo(long id, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(new EliminarTipoDocumento(id), ct);
        return resultado.EsExitoso ? NoContent() : BadRequest(new { error = resultado.Error });
    }

    [HttpGet("api/archivos/{id:long}")]
    public async Task<ActionResult<ArchivoDto>> ObtenerArchivo(long id, CancellationToken ct)
    {
        var archivo = await _despachador.ConsultarAsync(new ObtenerArchivoPorId(id), ct);
        return archivo is null ? NotFound() : Ok(archivo);
    }

    [HttpGet("api/archivos/directorio/{directorio}")]
    public async Task<ActionResult<IReadOnlyList<ArchivoDto>>> ListarArchivosPorDirectorio(string directorio, CancellationToken ct)
        => Ok(await _despachador.ConsultarAsync(new ListarArchivosPorDirectorio(directorio), ct));

    [HttpPost("api/archivos")]
    public async Task<IActionResult> RegistrarArchivo([FromBody] RegistrarArchivoRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(
            new RegistrarArchivo(req.Nombre, req.Extension, req.Directorio, req.IdTipoDocumento,
                req.Bytes, req.Version, req.Descripcion, Usuario, Host), ct);

        return resultado.EsExitoso
            ? Created($"api/archivos/{resultado.Valor}", new { id = resultado.Valor })
            : BadRequest(new { error = resultado.Error });
    }

    [HttpDelete("api/archivos/{id:long}")]
    public async Task<IActionResult> EliminarArchivo(long id, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(new EliminarArchivo(id), ct);
        return resultado.EsExitoso ? NoContent() : BadRequest(new { error = resultado.Error });
    }
}
