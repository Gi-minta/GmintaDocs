using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GmintaDocs.Api.Multitenancy;
using GmintaDocs.Identidad.Application;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GmintaDocs.Api.Seguridad;

public sealed record TokenEmitido(string Token, DateTime ExpiraEn);

/// <summary>Emite tokens JWT firmados (HMAC-SHA256) a partir de un usuario ya autenticado.</summary>
public sealed class GeneradorDeTokensJwt
{
    private readonly OpcionesDeJwt _opciones;

    public GeneradorDeTokensJwt(IOptions<OpcionesDeJwt> opciones) => _opciones = opciones.Value;

    public TokenEmitido Generar(UsuarioAutenticadoDto usuario, long idEmpresa, long? idSucursal)
    {
        var ahora = DateTime.UtcNow;
        var expira = ahora.AddMinutes(_opciones.MinutosDeVigencia);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(ClaimTypes.Name, usuario.UserName ?? usuario.Id),
            // La empresa viaja en el token: el middleware de tenant la lee de aquí.
            new(MiddlewareDeTenant.ClaimEmpresa, idEmpresa.ToString()),
        };

        if (idSucursal is { } sucursal)
            claims.Add(new Claim(MiddlewareDeTenant.ClaimSucursal, sucursal.ToString()));

        claims.AddRange(usuario.Roles.Select(rol => new Claim(ClaimTypes.Role, rol)));

        var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.ClaveSecreta));
        var credenciales = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _opciones.Emisor,
            audience: _opciones.Audiencia,
            claims: claims,
            notBefore: ahora,
            expires: expira,
            signingCredentials: credenciales);

        return new TokenEmitido(new JwtSecurityTokenHandler().WriteToken(token), expira);
    }
}
