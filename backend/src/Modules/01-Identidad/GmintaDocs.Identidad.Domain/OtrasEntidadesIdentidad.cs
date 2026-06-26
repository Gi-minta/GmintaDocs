using GmintaDocs.SharedKernel;

namespace GmintaDocs.Identidad.Domain;

/// <summary>Rol de seguridad (tabla roles).</summary>
public sealed class Rol : AggregateRoot<string>
{
    public string Nombre { get; private set; } = string.Empty; // columna "name"

    private Rol() { }

    private Rol(string id, string nombre) { Id = id; Nombre = nombre; }

    public static Result<Rol> Crear(string nombre, string? id = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return Result<Rol>.Fallido("El nombre del rol es obligatorio.");

        var identificador = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString() : id;
        return Result<Rol>.Exitoso(new Rol(identificador, nombre.Trim()));
    }

    public void Renombrar(string nombre) => Nombre = nombre.Trim();
}

/// <summary>Auditoría de cambios sobre tablas de identidad (tabla eventos_identidad).</summary>
public sealed class EventoIdentidad : AggregateRoot<long>
{
    public string Tabla { get; private set; } = string.Empty;
    public string IdRegistro { get; private set; } = string.Empty;
    public string Accion { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public string Host { get; private set; } = string.Empty;
    public string Comentario { get; private set; } = string.Empty;

    private EventoIdentidad() { }

    public EventoIdentidad(string tabla, string idRegistro, string accion, string usuario, string host, string comentario)
    {
        Tabla = tabla; IdRegistro = idRegistro; Accion = accion;
        Usuario = usuario; Host = host; Comentario = comentario; Fecha = DateTime.UtcNow;
    }
}

/// <summary>Sesión activa de un usuario (tabla online). Soporta la regla de sesión concurrente.</summary>
public sealed class SesionEnLinea : AggregateRoot<Guid>
{
    public string Usuario { get; private set; } = string.Empty;
    public int IdSucursal { get; private set; }

    private SesionEnLinea() { }

    public SesionEnLinea(Guid id, string usuario, int idSucursal)
    {
        Id = id; Usuario = usuario; IdSucursal = idSucursal;
    }
}

/// <summary>Valor de una opción de menú/permiso para un rol (tabla opciones_rol).</summary>
public sealed class OpcionRol : AggregateRoot<long>
{
    public string Rol { get; private set; } = string.Empty;
    public int IdOpcion { get; private set; }
    public string Valor { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public string Host { get; private set; } = string.Empty;

    private OpcionRol() { }

    public OpcionRol(string rol, int idOpcion, string valor, string usuario, string host)
    {
        Rol = rol; IdOpcion = idOpcion; Valor = valor; Usuario = usuario; Host = host; Fecha = DateTime.UtcNow;
    }

    public void CambiarValor(string valor) => Valor = valor;
}
