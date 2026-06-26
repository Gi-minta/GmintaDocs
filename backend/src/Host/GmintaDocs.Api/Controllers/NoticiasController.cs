using GmintaDocs.Colaboracion.Application;
using GmintaDocs.CQRS;
using GmintaDocs.Api.Seguridad;
using GmintaDocs.SharedKernel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GmintaDocs.Api.Controllers;

[Authorize(Policy = PoliticasDeAutorizacion.Gestion)]
[ApiController]
[Route("api/noticias")]
public sealed class NoticiasController : ControllerBase
{
    private readonly IDespachador _despachador;

    public NoticiasController(IDespachador despachador) => _despachador = despachador;

    private string Usuario => User?.Identity?.Name ?? "api";
    private string Host => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocido";

    public sealed record PublicarNoticiaRequest(string Titulo, string Autor, string? Avatar, string Texto);
    public sealed record ActualizarNoticiaRequest(string Titulo, string Texto);
    public sealed record ComentarNoticiaRequest(string Autor, string? Avatar, string Texto);

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<NoticiaDto>>> Listar(
        [FromQuery] int pagina = 1, [FromQuery] int tamano = 20, CancellationToken ct = default)
        => Ok(await _despachador.ConsultarAsync(new ListarNoticias(new ParametrosDePaginacion(pagina, tamano)), ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<NoticiaDto>> ObtenerPorId(long id, CancellationToken ct)
    {
        var noticia = await _despachador.ConsultarAsync(new ObtenerNoticiaPorId(id), ct);
        return noticia is null ? NotFound() : Ok(noticia);
    }

    [HttpPost]
    public async Task<IActionResult> Publicar([FromBody] PublicarNoticiaRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(
            new PublicarNoticia(req.Titulo, req.Autor, req.Avatar, req.Texto, Usuario, Host), ct);

        return resultado.EsExitoso
            ? CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Valor }, new { id = resultado.Valor })
            : BadRequest(new { error = resultado.Error });
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Actualizar(long id, [FromBody] ActualizarNoticiaRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(new ActualizarNoticia(id, req.Titulo, req.Texto), ct);
        return resultado.EsExitoso ? NoContent() : BadRequest(new { error = resultado.Error });
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Eliminar(long id, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(new EliminarNoticia(id), ct);
        return resultado.EsExitoso ? NoContent() : BadRequest(new { error = resultado.Error });
    }

    [HttpGet("{id:long}/comentarios")]
    public async Task<ActionResult<IReadOnlyList<ComentarioDto>>> ListarComentarios(long id, CancellationToken ct)
        => Ok(await _despachador.ConsultarAsync(new ListarComentariosDeNoticia(id), ct));

    [HttpPost("{id:long}/comentarios")]
    public async Task<IActionResult> Comentar(long id, [FromBody] ComentarNoticiaRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(
            new ComentarNoticia(id, req.Autor, req.Avatar, req.Texto, Usuario, Host), ct);

        return resultado.EsExitoso
            ? Created($"api/noticias/{id}/comentarios/{resultado.Valor}", new { id = resultado.Valor })
            : BadRequest(new { error = resultado.Error });
    }
}
