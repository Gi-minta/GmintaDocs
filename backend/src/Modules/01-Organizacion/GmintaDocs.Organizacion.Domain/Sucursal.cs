using GmintaDocs.SharedKernel;

namespace GmintaDocs.Organizacion.Domain;

/// <summary>
/// Sucursal (sede) de una empresa. Soporta el pilar multi-sede del sistema.
/// </summary>
public sealed class Sucursal : AggregateRoot<long>
{
    public long IdEmpresa { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string Direccion { get; private set; } = string.Empty;
    public string Telefono { get; private set; } = string.Empty;
    public bool Activa { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;

    private Sucursal() { }

    private Sucursal(long idEmpresa, string codigo, string nombre, string direccion,
        string telefono, string usuario, string host)
    {
        IdEmpresa = idEmpresa;
        Codigo = codigo;
        Nombre = nombre;
        Direccion = direccion;
        Telefono = telefono;
        Activa = true;
        Usuario = usuario;
        Host = host;
        Fecha = DateTime.UtcNow;
    }

    public static Result<Sucursal> Crear(long idEmpresa, string codigo, string nombre,
        string direccion, string telefono, string usuario, string host)
    {
        if (idEmpresa <= 0)
            return Result<Sucursal>.Fallido("La sucursal debe pertenecer a una empresa válida.");
        if (string.IsNullOrWhiteSpace(codigo))
            return Result<Sucursal>.Fallido("El código de la sucursal es obligatorio.");
        if (string.IsNullOrWhiteSpace(nombre))
            return Result<Sucursal>.Fallido("El nombre de la sucursal es obligatorio.");

        return Result<Sucursal>.Exitoso(
            new Sucursal(idEmpresa, codigo.Trim(), nombre.Trim(), direccion ?? string.Empty,
                telefono ?? string.Empty, usuario, host));
    }

    public void Activar() => Activa = true;
    public void Desactivar() => Activa = false;
}
