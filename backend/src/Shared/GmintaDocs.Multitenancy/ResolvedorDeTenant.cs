using Microsoft.Extensions.Configuration;

namespace GmintaDocs.Multitenancy;

/// <summary>
/// Resuelve la cadena de conexión de la base de datos dedicada de una empresa.
/// </summary>
/// <remarks>
/// DESVIACIÓN respecto al DDL (GmintaDocs.sql): la tabla <c>empresas</c> no tiene una
/// columna de cadena de conexión, por lo que el <c>CadenaCnx</c> de la regla 5.6 se resuelve
/// desde configuración. Se busca primero una entrada explícita en la sección
/// <c>Tenants:{IdEmpresa}</c> y, si no existe, se construye a partir de la plantilla
/// <c>ConnectionStrings:PlantillaTenant</c> sustituyendo el token <c>{IdEmpresa}</c>.
/// </remarks>
public interface IResolvedorDeTenant
{
    string ResolverCadenaDeConexion(long idEmpresa);
}

public sealed class ResolvedorDeTenantDesdeConfiguracion : IResolvedorDeTenant
{
    private readonly IConfiguration _configuracion;

    public ResolvedorDeTenantDesdeConfiguracion(IConfiguration configuracion)
    {
        _configuracion = configuracion;
    }

    public string ResolverCadenaDeConexion(long idEmpresa)
    {
        var explicita = _configuracion[$"Tenants:{idEmpresa}"];
        if (!string.IsNullOrWhiteSpace(explicita))
            return explicita;

        var plantilla = _configuracion.GetConnectionString("PlantillaTenant");
        if (!string.IsNullOrWhiteSpace(plantilla))
            return plantilla.Replace("{IdEmpresa}", idEmpresa.ToString());

        throw new InvalidOperationException(
            $"No se pudo resolver la cadena de conexión para la empresa {idEmpresa}. " +
            "Configure 'Tenants:{id}' o 'ConnectionStrings:PlantillaTenant'.");
    }
}
