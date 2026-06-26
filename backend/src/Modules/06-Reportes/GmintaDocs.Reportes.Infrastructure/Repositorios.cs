using GmintaDocs.Multitenancy;
using GmintaDocs.Reportes.Application;
using GmintaDocs.Reportes.Domain;
using GmintaDocs.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace GmintaDocs.Reportes.Infrastructure;

public sealed class RepositorioDeCategoriasReporte : RepositorioEfBase<CategoriaReporte, long>, IRepositorioDeCategoriasReporte
{
    public RepositorioDeCategoriasReporte(ReportesDbContext contexto) : base(contexto) { }

    public Task<ResultadoPaginado<CategoriaReporte>> ListarPaginadoAsync(ParametrosDePaginacion parametros, CancellationToken cancellationToken = default)
        => Conjunto.AsNoTracking().OrderBy(c => c.Categoria).PaginarAsync(parametros, cancellationToken);
}

public sealed class RepositorioDeReportes : RepositorioEfBase<ReporteSsrs, long>, IRepositorioDeReportes
{
    public RepositorioDeReportes(ReportesDbContext contexto) : base(contexto) { }

    public async Task<IReadOnlyList<ReporteSsrs>> ListarPorCategoriaAsync(long idCategoria, CancellationToken cancellationToken = default)
        => await Conjunto.AsNoTracking()
            .Where(r => r.IdCategoria == idCategoria)
            .OrderBy(r => r.Reporte)
            .ToListAsync(cancellationToken);
}
