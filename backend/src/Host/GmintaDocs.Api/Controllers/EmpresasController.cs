using GmintaDocs.Api.Seguridad;
using GmintaDocs.CQRS;
using GmintaDocs.Organizacion.Application;
using GmintaDocs.SharedKernel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GmintaDocs.Api.Controllers;

[ApiController]
[Route("api/empresas")]
[Authorize(Roles = RolesDelSistema.Administrador)]
public sealed class EmpresasController : ControllerBase
{
    private readonly IDespachador _despachador;

    public EmpresasController(IDespachador despachador) => _despachador = despachador;

    private string Usuario => User?.Identity?.Name ?? "api";
    private string Host => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocido";

    public sealed record CrearEmpresaRequest(string RazonSocial, string Nit);
    public sealed record ActualizarEmpresaRequest(string RazonSocial, string? Direccion, string? Ciudad,
        string? Url, string? Email, string? Telefono, string? Notas);
    public sealed record CrearSucursalRequest(string Codigo, string Nombre, string Direccion, string Telefono);

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<EmpresaDto>>> Listar(
        [FromQuery] int pagina = 1, [FromQuery] int tamano = 20, CancellationToken ct = default)
        => Ok(await _despachador.ConsultarAsync(new ListarEmpresas(new ParametrosDePaginacion(pagina, tamano)), ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<EmpresaDto>> ObtenerPorId(long id, CancellationToken ct)
    {
        var empresa = await _despachador.ConsultarAsync(new ObtenerEmpresaPorId(id), ct);
        return empresa is null ? NotFound() : Ok(empresa);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearEmpresaRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(
            new CrearEmpresa(req.RazonSocial, req.Nit, Usuario, Host), ct);

        return resultado.EsExitoso
            ? CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Valor }, new { id = resultado.Valor })
            : BadRequest(new { error = resultado.Error });
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Actualizar(long id, [FromBody] ActualizarEmpresaRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(
            new ActualizarEmpresa(id, req.RazonSocial, req.Direccion, req.Ciudad, req.Url, req.Email, req.Telefono, req.Notas), ct);

        return resultado.EsExitoso ? NoContent() : BadRequest(new { error = resultado.Error });
    }

    [HttpGet("{id:long}/sucursales")]
    public async Task<ActionResult<IReadOnlyList<SucursalDto>>> ListarSucursales(long id, CancellationToken ct)
        => Ok(await _despachador.ConsultarAsync(new ListarSucursalesDeEmpresa(id), ct));

    [HttpPost("{id:long}/sucursales")]
    public async Task<IActionResult> CrearSucursal(long id, [FromBody] CrearSucursalRequest req, CancellationToken ct)
    {
        var resultado = await _despachador.EnviarAsync(
            new CrearSucursal(id, req.Codigo, req.Nombre, req.Direccion, req.Telefono, Usuario, Host), ct);

        return resultado.EsExitoso
            ? Created($"api/empresas/{id}/sucursales/{resultado.Valor}", new { id = resultado.Valor })
            : BadRequest(new { error = resultado.Error });
    }
}
