using GmintaDocs.Multitenancy;
using GmintaDocs.Plantillas.Application;
using GmintaDocs.Plantillas.Domain;
using GmintaDocs.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace GmintaDocs.Plantillas.Infrastructure;

public sealed class RepositorioDePlantillas : RepositorioEfBase<Plantilla, int>, IRepositorioDePlantillas
{
    public RepositorioDePlantillas(PlantillasDbContext contexto) : base(contexto) { }

    public Task<Plantilla?> ObtenerPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
        => Conjunto.FirstOrDefaultAsync(p => p.Codigo == codigo, cancellationToken);

    public Task<ResultadoPaginado<Plantilla>> ListarPaginadoAsync(ParametrosDePaginacion parametros, CancellationToken cancellationToken = default)
        => Conjunto.AsNoTracking().OrderBy(p => p.Nombre).PaginarAsync(parametros, cancellationToken);
}

public sealed class RepositorioDePlantillasFormato : RepositorioEfBase<PlantillaFormato, int>, IRepositorioDePlantillasFormato
{
    public RepositorioDePlantillasFormato(PlantillasDbContext contexto) : base(contexto) { }

    public async Task<IReadOnlyList<PlantillaFormato>> ListarAsync(CancellationToken cancellationToken = default)
        => await Conjunto.AsNoTracking().OrderBy(p => p.Nombre).ToListAsync(cancellationToken);
}
