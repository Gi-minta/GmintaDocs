namespace GmintaDocs.Multitenancy;

/// <summary>
/// Implementación scoped del contexto de tenant. Se rellena al inicio del request
/// (por el middleware de resolución de tenant) antes de tocar cualquier DbContext por empresa.
/// </summary>
public sealed class ContextoDeTenant : IContextoDeTenant
{
    public long IdEmpresa { get; private set; }
    public string NombreEmpresa { get; private set; } = string.Empty;
    public string CadenaDeConexion { get; private set; } = string.Empty;
    public long? IdSucursal { get; private set; }

    public bool EstaResuelto { get; private set; }

    public void Establecer(long idEmpresa, string nombreEmpresa, string cadenaDeConexion, long? idSucursal)
    {
        IdEmpresa = idEmpresa;
        NombreEmpresa = nombreEmpresa;
        CadenaDeConexion = cadenaDeConexion;
        IdSucursal = idSucursal;
        EstaResuelto = true;
    }
}
