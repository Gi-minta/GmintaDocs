using GmintaDocs.CQRS;
using GmintaDocs.Reportes.Application;
using GmintaDocs.Api.Seguridad;
using GmintaDocs.SharedKernel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GmintaDocs.Api.Controllers;

[Authorize(Policy = PoliticasDeAutorizacion.Gestion)]
[ApiController]
public sealed class ReportesController : ControllerBase
{
    private readonly IDespachador _despachador;

    public ReportesController(IDespachador despachador) => _despachador = despachador;

    private string Usuario => User?.Identity?.Name ?? "api";
    private string Host => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocido";

    public sealed record CrearCategoriaRequest(string Codigo, string Categoria, string? Descripcion);
    public sealed record CrearReporteRequest(long IdCategoria, string Codigo, string Reporte, string Url, string? Descripcion);
    public sealed record ActualizarReporteRequest(string Reporte, string Url, string? Descripcion);

    [HttpGet("api/categorias-reporte")]
    public async Task<ActionResult<ResultadoPaginado<CategoriaReporteDto>>> ListarCategorias(
        [FromQuery] int pagina = 1, [FromQuery] int tamano = 20, CancellationToken ct = default)
        => Ok(await _despachador.ConsultarAsync(new ListarCategoriasReporte(new ParametrosDePaginacion(pagina, tamano)), ct));

    [HttpPost("api/categorias-reporte")]
    public async Task<IActionResult> CrearCategoria([FromBody] CrearCategoriaRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(
            new CrearCategoriaReporte(req.Codigo, req.Categoria, req.Descripcion, Usuario, Host), ct);

        return resultado.EsExitoso
            ? Created($"api/categorias-reporte/{resultado.Valor}", new { id = resultado.Valor })
            : BadRequest(new { error = resultado.Error });
    }

    [HttpGet("api/categorias-reporte/{idCategoria:long}/reportes")]
    public async Task<ActionResult<IReadOnlyList<ReporteDto>>> ListarPorCategoria(long idCategoria, CancellationToken ct)
        => Ok(await _despachador.ConsultarAsync(new ListarReportesPorCategoria(idCategoria), ct));

    [HttpPost("api/reportes")]
    public async Task<IActionResult> CrearReporte([FromBody] CrearReporteRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(
            new CrearReporte(req.IdCategoria, req.Codigo, req.Reporte, req.Url, req.Descripcion, Usuario, Host), ct);

        return resultado.EsExitoso
            ? Created($"api/reportes/{resultado.Valor}", new { id = resultado.Valor })
            : BadRequest(new { error = resultado.Error });
    }

    [HttpPut("api/reportes/{id:long}")]
    public async Task<IActionResult> ActualizarReporte(long id, [FromBody] ActualizarReporteRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(new ActualizarReporte(id, req.Reporte, req.Url, req.Descripcion), ct);
        return resultado.EsExitoso ? NoContent() : BadRequest(new { error = resultado.Error });
    }

    [HttpDelete("api/reportes/{id:long}")]
    public async Task<IActionResult> EliminarReporte(long id, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(new EliminarReporte(id), ct);
        return resultado.EsExitoso ? NoContent() : BadRequest(new { error = resultado.Error });
    }
}
