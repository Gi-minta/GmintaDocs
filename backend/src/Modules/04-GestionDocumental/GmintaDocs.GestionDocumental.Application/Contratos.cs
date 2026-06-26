using GmintaDocs.GestionDocumental.Domain;
using GmintaDocs.SharedKernel;

namespace GmintaDocs.GestionDocumental.Application;

/// <summary>Unidad de trabajo del módulo (evita colisión de DI entre los DbContext por módulo).</summary>
public interface IUnidadDeTrabajoGestionDocumental : IUnidadDeTrabajo { }

public interface IRepositorioDeArchivos : IRepositorio<Archivo, long>
{
    Task<IReadOnlyList<Archivo>> ListarPorDirectorioAsync(string directorio, CancellationToken cancellationToken = default);
}

public interface IRepositorioDeTiposDocumento : IRepositorio<TipoDocumento, long>
{
    Task<TipoDocumento?> ObtenerPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);
    Task<ResultadoPaginado<TipoDocumento>> ListarPaginadoAsync(ParametrosDePaginacion parametros, CancellationToken cancellationToken = default);
}

public sealed record ArchivoDto(long Id, string Nombre, string Extension, string Directorio, short Estado,
    string Version, bool EsVersionActual, long IdTipoDocumento, long Bytes);

public sealed record TipoDocumentoDto(long Id, string Codigo, string Nombre, bool ControlaVersion,
    int DiasVigencia, bool ControlaVigencia);

/// <summary>Mapeos explícitos dominio → DTO (sin AutoMapper, por decisión de stack).</summary>
public static class MapeosGestionDocumental
{
    public static ArchivoDto ADto(this Archivo a) =>
        new(a.Id, a.Nombre, a.Extension, a.Directorio, a.Estado, a.Version, a.EsVersionActual, a.IdTipoDocumento, a.Bytes);

    public static TipoDocumentoDto ADto(this TipoDocumento t) =>
        new(t.Id, t.Codigo, t.Nombre, t.ControlaVersion, t.DiasVigencia, t.ControlaVigencia);
}
