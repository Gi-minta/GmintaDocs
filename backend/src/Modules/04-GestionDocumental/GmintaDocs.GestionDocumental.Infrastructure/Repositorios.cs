using GmintaDocs.GestionDocumental.Application;
using GmintaDocs.GestionDocumental.Domain;
using GmintaDocs.Multitenancy;
using GmintaDocs.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace GmintaDocs.GestionDocumental.Infrastructure;

public sealed class RepositorioDeArchivos : RepositorioEfBase<Archivo, long>, IRepositorioDeArchivos
{
    public RepositorioDeArchivos(GestionDocumentalDbContext contexto) : base(contexto) { }

    public async Task<IReadOnlyList<Archivo>> ListarPorDirectorioAsync(string directorio, CancellationToken cancellationToken = default)
        => await Conjunto.AsNoTracking()
            .Where(a => a.Directorio == directorio && a.EsVersionActual)
            .OrderByDescending(a => a.FechaPublicacion)
            .ToListAsync(cancellationToken);
}

public sealed class RepositorioDeTiposDocumento : RepositorioEfBase<TipoDocumento, long>, IRepositorioDeTiposDocumento
{
    public RepositorioDeTiposDocumento(GestionDocumentalDbContext contexto) : base(contexto) { }

    public Task<TipoDocumento?> ObtenerPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
        => Conjunto.FirstOrDefaultAsync(t => t.Codigo == codigo, cancellationToken);

    public Task<ResultadoPaginado<TipoDocumento>> ListarPaginadoAsync(ParametrosDePaginacion parametros, CancellationToken cancellationToken = default)
        => Conjunto.AsNoTracking().OrderBy(t => t.Nombre).PaginarAsync(parametros, cancellationToken);
}
