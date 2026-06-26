namespace GmintaDocs.Multitenancy;

/// <summary>
/// Representa la empresa (tenant) activa en el request/operación actual,
/// incluida la cadena de conexión a su base de datos dedicada (CadenaCnx).
/// </summary>
public interface IContextoDeTenant
{
    long IdEmpresa { get; }
    string NombreEmpresa { get; }
    string CadenaDeConexion { get; }
    long? IdSucursal { get; }
}
