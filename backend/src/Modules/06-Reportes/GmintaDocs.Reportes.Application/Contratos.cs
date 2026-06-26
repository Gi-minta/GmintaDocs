using GmintaDocs.Reportes.Domain;
using GmintaDocs.SharedKernel;

namespace GmintaDocs.Reportes.Application;

/// <summary>Unidad de trabajo del módulo (evita colisión de DI entre los DbContext por módulo).</summary>
public interface IUnidadDeTrabajoReportes : IUnidadDeTrabajo { }

public interface IRepositorioDeCategoriasReporte : IRepositorio<CategoriaReporte, long>
{
    Task<ResultadoPaginado<CategoriaReporte>> ListarPaginadoAsync(ParametrosDePaginacion parametros, CancellationToken cancellationToken = default);
}

public interface IRepositorioDeReportes : IRepositorio<ReporteSsrs, long>
{
    Task<IReadOnlyList<ReporteSsrs>> ListarPorCategoriaAsync(long idCategoria, CancellationToken cancellationToken = default);
}

public sealed record CategoriaReporteDto(long Id, string Codigo, string Categoria, string? Descripcion);
public sealed record ReporteDto(long Id, long IdCategoria, string Codigo, string Reporte, string Url, string? Descripcion);

/// <summary>Mapeos explícitos dominio → DTO (sin AutoMapper, por decisión de stack).</summary>
public static class MapeosReportes
{
    public static CategoriaReporteDto ADto(this CategoriaReporte c) =>
        new(c.Id, c.Codigo, c.Categoria, c.Descripcion);

    public static ReporteDto ADto(this ReporteSsrs r) =>
        new(r.Id, r.IdCategoria, r.Codigo, r.Reporte, r.Url, r.Descripcion);
}
