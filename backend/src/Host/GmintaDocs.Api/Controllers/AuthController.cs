using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GmintaDocs.Api.Seguridad;
using GmintaDocs.CQRS;
using GmintaDocs.Identidad.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GmintaDocs.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IDespachador _despachador;
    private readonly GeneradorDeTokensJwt _generador;

    public AuthController(IDespachador despachador, GeneradorDeTokensJwt generador)
    {
        _despachador = despachador;
        _generador = generador;
    }

    public sealed record LoginRequest(string UserName, string Contrasena, long IdEmpresa, long? IdSucursal);
    public sealed record LoginRespuesta(string Token, DateTime ExpiraEn, UsuarioAutenticadoDto Usuario);
    public sealed record CambiarContrasenaRequest(string ContrasenaActual, string ContrasenaNueva);

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        if (req.IdEmpresa <= 0)
            return BadRequest(new { error = "Debe indicar la empresa (IdEmpresa)." });

        var resultado = await _despachador.ConsultarAsync(
            new AutenticarUsuario(req.UserName, req.Contrasena), ct);

        if (!resultado.EsExitoso)
            return Unauthorized(new { error = resultado.Error });

        var token = _generador.Generar(resultado.Valor, req.IdEmpresa, req.IdSucursal);
        return Ok(new LoginRespuesta(token.Token, token.ExpiraEn, resultado.Valor));
    }

    /// <summary>Cambia la contraseña del usuario autenticado (requiere su contraseña actual).</summary>
    [HttpPost("cambiar-contrasena")]
    [Authorize]
    public async Task<IActionResult> CambiarContrasena([FromBody] CambiarContrasenaRequest req, CancellationToken ct)
    {
        var idUsuario = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (string.IsNullOrEmpty(idUsuario))
            return Unauthorized();

        var resultado = await _despachador.EnviarAsync(
            new GmintaDocs.Identidad.Application.CambiarContrasena(idUsuario, req.ContrasenaActual, req.ContrasenaNueva), ct);

        return resultado.EsExitoso ? NoContent() : BadRequest(new { error = resultado.Error });
    }
}
