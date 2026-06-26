using GmintaDocs.SharedKernel;

namespace GmintaDocs.AdminFormularios.Domain;

/// <summary>Definición de un formulario dinámico (tabla formularios).</summary>
public sealed class Formulario : AggregateRoot<long>
{
    public string Codigo { get; private set; } = string.Empty;
    public string Tabla { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public long IdPadre { get; private set; }
    public short Estado { get; private set; }
    public bool Imagen { get; private set; }
    public short LongRadicado { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;

    private Formulario() { }

    public static Result<Formulario> Crear(string codigo, string tabla, string nombre, string usuario, string host)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return Result<Formulario>.Fallido("El código es obligatorio.");
        if (string.IsNullOrWhiteSpace(nombre)) return Result<Formulario>.Fallido("El nombre es obligatorio.");

        return Result<Formulario>.Exitoso(new Formulario
        {
            Codigo = codigo.Trim(), Tabla = tabla, Nombre = nombre.Trim(),
            Usuario = usuario, Host = host, Fecha = DateTime.UtcNow
        });
    }

    public Result Editar(string nombre, string? descripcion)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return Result.Fallido("El nombre es obligatorio.");
        Nombre = nombre.Trim();
        Descripcion = descripcion;
        return Result.Exitoso();
    }
}

/// <summary>Campo (columna lógica) de un formulario (tabla campos).</summary>
public sealed class Campo : AggregateRoot<long>
{
    public long IdFormulario { get; private set; }
    public int Orden { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string Columna { get; private set; } = string.Empty;
    public short TipoDato { get; private set; }
    public int LongDato { get; private set; }
    public short Control { get; private set; }
    public short Estado { get; private set; }
    public bool Unico { get; private set; }
    public bool Mostrar { get; private set; }
    public string? Mascara { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;
    public bool Requerido { get; private set; }
    public short? Enlace { get; private set; }
    public long? IdEnlace { get; private set; }
    public string? CascadaDe { get; private set; }
    public string? Campo1 { get; private set; }
    public string? Campo2 { get; private set; }
    public bool Sticker { get; private set; }

    private Campo() { }

    public static Result<Campo> Crear(long idFormulario, int orden, string nombre, string columna,
        short tipoDato, int longDato, short control, bool requerido, string usuario, string host)
    {
        if (idFormulario <= 0) return Result<Campo>.Fallido("El campo debe pertenecer a un formulario válido.");
        if (string.IsNullOrWhiteSpace(nombre)) return Result<Campo>.Fallido("El nombre del campo es obligatorio.");
        if (string.IsNullOrWhiteSpace(columna)) return Result<Campo>.Fallido("La columna del campo es obligatoria.");

        return Result<Campo>.Exitoso(new Campo
        {
            IdFormulario = idFormulario, Orden = orden, Nombre = nombre.Trim(), Columna = columna.Trim(),
            TipoDato = tipoDato, LongDato = longDato, Control = control, Estado = 1, Requerido = requerido,
            Mostrar = true, Usuario = usuario, Host = host, Fecha = DateTime.UtcNow
        });
    }
}

/// <summary>Copia de un formulario a un directorio (tabla copias).</summary>
public sealed class Copia : AggregateRoot<long>
{
    public long IdFormulario { get; private set; }
    public long IdDirectorio { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;

    private Copia() { }
}

/// <summary>Lista de valores (tabla lista).</summary>
public sealed class Lista : AggregateRoot<long>
{
    public string Nombre { get; private set; } = string.Empty;
    public long IdListaPadre { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;

    private Lista() { }
}

/// <summary>Ítem de una lista de valores (tabla item_lista).</summary>
public sealed class ItemLista : AggregateRoot<long>
{
    public long IdLista { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public long IdItemPadre { get; private set; }
    public bool Activo { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;

    private ItemLista() { }
}

/// <summary>Plantilla de mensaje de notificación (tabla mensajes_notificacion).</summary>
public sealed class MensajeNotificacion : AggregateRoot<int>
{
    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string Mensaje { get; private set; } = string.Empty;
    public int Estado { get; private set; }
    public long IdFormulario { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;

    private MensajeNotificacion() { }
}

/// <summary>Parámetro de una plantilla de mensaje (tabla parametros_mensaje).</summary>
public sealed class ParametroMensaje : AggregateRoot<int>
{
    public int IdPlantilla { get; private set; }
    public string Parametro { get; private set; } = string.Empty;
    public string Tabla { get; private set; } = string.Empty;
    public string Campo { get; private set; } = string.Empty;
    public string Usuario { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;

    private ParametroMensaje() { }
}
