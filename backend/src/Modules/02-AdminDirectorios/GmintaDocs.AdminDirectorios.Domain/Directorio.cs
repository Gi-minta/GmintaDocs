using GmintaDocs.SharedKernel;

namespace GmintaDocs.AdminDirectorios.Domain;

/// <summary>Directorio (carpeta) jerárquico asociado a un formulario (tabla directorios).</summary>
public sealed class Directorio : AggregateRoot<long>
{
    public long IdFormulario { get; private set; }
    public long IdDirectorioPadre { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public int IdEstado { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;
    public long? IdDirectorioV10 { get; private set; }

    private Directorio() { }

    public static Result<Directorio> Crear(long idFormulario, long idDirectorioPadre, string codigo,
        string nombre, string usuario, string host)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return Result<Directorio>.Fallido("El nombre del directorio es obligatorio.");

        return Result<Directorio>.Exitoso(new Directorio
        {
            IdFormulario = idFormulario, IdDirectorioPadre = idDirectorioPadre,
            Codigo = codigo, Nombre = nombre.Trim(), Usuario = usuario, Host = host, Fecha = DateTime.UtcNow
        });
    }

    public Result Editar(string nombre, string codigo)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return Result.Fallido("El nombre del directorio es obligatorio.");
        Nombre = nombre.Trim();
        Codigo = codigo;
        return Result.Exitoso();
    }
}
