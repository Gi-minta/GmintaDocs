using GmintaDocs.SharedKernel;

namespace GmintaDocs.Reportes.Domain;

/// <summary>Categoría de reportes (tabla categoria_reportes).</summary>
public sealed class CategoriaReporte : AggregateRoot<long>
{
    public string Codigo { get; private set; } = string.Empty;
    public string Categoria { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;

    private CategoriaReporte() { }

    public static Result<CategoriaReporte> Crear(string codigo, string categoria, string? descripcion,
        string usuario, string host)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return Result<CategoriaReporte>.Fallido("El código es obligatorio.");
        if (string.IsNullOrWhiteSpace(categoria)) return Result<CategoriaReporte>.Fallido("La categoría es obligatoria.");

        return Result<CategoriaReporte>.Exitoso(new CategoriaReporte
        {
            Codigo = codigo.Trim(), Categoria = categoria.Trim(), Descripcion = descripcion,
            Usuario = usuario, Host = host, Fecha = DateTime.UtcNow
        });
    }
}

/// <summary>Reporte SSRS publicado (tabla reportes_ssrs).</summary>
public sealed class ReporteSsrs : AggregateRoot<long>
{
    public long IdCategoria { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Reporte { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public string Usuario { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;

    private ReporteSsrs() { }

    public static Result<ReporteSsrs> Crear(long idCategoria, string codigo, string reporte, string url,
        string? descripcion, string usuario, string host)
    {
        if (idCategoria <= 0) return Result<ReporteSsrs>.Fallido("El reporte debe pertenecer a una categoría válida.");
        if (string.IsNullOrWhiteSpace(codigo)) return Result<ReporteSsrs>.Fallido("El código es obligatorio.");
        if (string.IsNullOrWhiteSpace(reporte)) return Result<ReporteSsrs>.Fallido("El nombre del reporte es obligatorio.");
        if (string.IsNullOrWhiteSpace(url)) return Result<ReporteSsrs>.Fallido("La URL del reporte es obligatoria.");

        return Result<ReporteSsrs>.Exitoso(new ReporteSsrs
        {
            IdCategoria = idCategoria, Codigo = codigo.Trim(), Reporte = reporte.Trim(), Url = url.Trim(),
            Descripcion = descripcion, Usuario = usuario, Host = host, Fecha = DateTime.UtcNow
        });
    }

    public Result Editar(string reporte, string url, string? descripcion)
    {
        if (string.IsNullOrWhiteSpace(reporte)) return Result.Fallido("El nombre del reporte es obligatorio.");
        if (string.IsNullOrWhiteSpace(url)) return Result.Fallido("La URL del reporte es obligatoria.");
        Reporte = reporte.Trim();
        Url = url.Trim();
        Descripcion = descripcion;
        return Result.Exitoso();
    }
}
