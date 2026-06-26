using GmintaDocs.Multitenancy;

namespace GmintaDocs.Api.Multitenancy;

/// <summary>
/// Resuelve la empresa (tenant) activa al inicio de cada request y rellena el
/// <see cref="ContextoDeTenant"/> scoped antes de que cualquier DbContext por empresa se utilice.
/// </summary>
/// <remarks>
/// Orden de resolución de la empresa:
/// <list type="number">
/// <item>cabecera <c>X-Id-Empresa</c> (útil para herramientas/administración), y si no</item>
/// <item>el claim <c>id_empresa</c> del token JWT del usuario autenticado.</item>
/// </list>
/// Debe ejecutarse <b>después</b> de <c>UseAuthentication</c> para que el claim esté disponible.
/// </remarks>
public sealed class MiddlewareDeTenant
{
    public const string CabeceraEmpresa = "X-Id-Empresa";
    public const string CabeceraSucursal = "X-Id-Sucursal";
    public const string ClaimEmpresa = "id_empresa";
    public const string ClaimSucursal = "id_sucursal";

    private readonly RequestDelegate _siguiente;

    public MiddlewareDeTenant(RequestDelegate siguiente) => _siguiente = siguiente;

    public async Task InvokeAsync(HttpContext contexto, ContextoDeTenant tenant, IResolvedorDeTenant resolvedor)
    {
        if (TryResolverEmpresa(contexto, out var idEmpresa))
        {
            var cadena = resolvedor.ResolverCadenaDeConexion(idEmpresa);
            var idSucursal = ResolverSucursal(contexto);
            tenant.Establecer(idEmpresa, $"Empresa {idEmpresa}", cadena, idSucursal);
        }

        await _siguiente(contexto);
    }

    private static bool TryResolverEmpresa(HttpContext contexto, out long idEmpresa)
    {
        if (contexto.Request.Headers.TryGetValue(CabeceraEmpresa, out var valor) &&
            long.TryParse(valor, out idEmpresa) && idEmpresa > 0)
            return true;

        var claim = contexto.User?.FindFirst(ClaimEmpresa)?.Value;
        if (long.TryParse(claim, out idEmpresa) && idEmpresa > 0)
            return true;

        idEmpresa = 0;
        return false;
    }

    private static long? ResolverSucursal(HttpContext contexto)
    {
        if (contexto.Request.Headers.TryGetValue(CabeceraSucursal, out var valor) &&
            long.TryParse(valor, out var sucursal))
            return sucursal;

        var claim = contexto.User?.FindFirst(ClaimSucursal)?.Value;
        return long.TryParse(claim, out sucursal) ? sucursal : null;
    }
}
