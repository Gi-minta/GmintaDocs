using GmintaDocs.SharedKernel;

namespace GmintaDocs.Plantillas.Domain;

/// <summary>Plantilla genérica de texto/HTML (tabla plantillas).</summary>
public sealed class Plantilla : AggregateRoot<int>
{
    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string Contenido { get; private set; } = string.Empty; // columna "plantilla"

    private Plantilla() { }

    public static Result<Plantilla> Crear(string codigo, string nombre, string? contenido)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return Result<Plantilla>.Fallido("El código es obligatorio.");
        if (string.IsNullOrWhiteSpace(nombre)) return Result<Plantilla>.Fallido("El nombre es obligatorio.");

        return Result<Plantilla>.Exitoso(new Plantilla
        {
            Codigo = codigo.Trim(), Nombre = nombre.Trim(), Contenido = contenido ?? string.Empty
        });
    }

    public Result Editar(string nombre, string? contenido)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return Result.Fallido("El nombre es obligatorio.");
        Nombre = nombre.Trim();
        Contenido = contenido ?? string.Empty;
        return Result.Exitoso();
    }
}

/// <summary>Plantilla de formato HTML asociada a un formulario (tabla plantillas_formato).</summary>
public sealed class PlantillaFormato : AggregateRoot<int>
{
    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string FormatoHtml { get; private set; } = string.Empty;
    public int Estado { get; private set; }
    public long IdFormulario { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;

    private PlantillaFormato() { }

    public static Result<PlantillaFormato> Crear(string codigo, string nombre, string? formatoHtml,
        long idFormulario, string usuario, string host)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return Result<PlantillaFormato>.Fallido("El código es obligatorio.");
        if (string.IsNullOrWhiteSpace(nombre)) return Result<PlantillaFormato>.Fallido("El nombre es obligatorio.");

        return Result<PlantillaFormato>.Exitoso(new PlantillaFormato
        {
            Codigo = codigo.Trim(), Nombre = nombre.Trim(), FormatoHtml = formatoHtml ?? string.Empty,
            Estado = 1, IdFormulario = idFormulario, Usuario = usuario, Host = host, Fecha = DateTime.UtcNow
        });
    }
}

/// <summary>Parámetro (binding tabla.campo) de una plantilla de formato (tabla parametros_formato).</summary>
public sealed class ParametroFormato : AggregateRoot<int>
{
    public int IdPlantilla { get; private set; }
    public string Parametro { get; private set; } = string.Empty;
    public string Tabla { get; private set; } = string.Empty;
    public string Campo { get; private set; } = string.Empty;
    public string Usuario { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;

    private ParametroFormato() { }
}
