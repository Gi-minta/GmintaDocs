using System.Text;
using Microsoft.EntityFrameworkCore;

namespace GmintaDocs.Multitenancy;

/// <summary>
/// Convenciones de mapeo compartidas por los <c>DbContext</c> de los módulos.
/// Convierte los nombres PascalCase de propiedades a columnas <c>snake_case</c>
/// para coincidir con el DDL de PostgreSQL (GmintaDocs.sql).
/// </summary>
public static class ConvencionesDeNomenclatura
{
    /// <summary>Aplica nomenclatura snake_case a todas las columnas del modelo.</summary>
    public static void AplicarSnakeCaseAColumnas(ModelBuilder modelBuilder)
    {
        foreach (var entidad in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var propiedad in entidad.GetProperties())
            {
                // Respeta nombres de columna ya configurados explícitamente.
                if (propiedad.GetColumnName() == propiedad.Name)
                    propiedad.SetColumnName(ASnakeCase(propiedad.Name));
            }
        }
    }

    public static string ASnakeCase(string nombre)
    {
        if (string.IsNullOrEmpty(nombre)) return nombre;

        var sb = new StringBuilder(nombre.Length + 8);
        for (var i = 0; i < nombre.Length; i++)
        {
            var c = nombre[i];
            if (char.IsUpper(c) && i > 0 &&
                (char.IsLower(nombre[i - 1]) || char.IsDigit(nombre[i - 1])))
            {
                sb.Append('_');
            }
            sb.Append(char.ToLowerInvariant(c));
        }
        return sb.ToString();
    }
}
