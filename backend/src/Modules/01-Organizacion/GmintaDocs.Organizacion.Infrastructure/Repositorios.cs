using GmintaDocs.Multitenancy;
using GmintaDocs.Organizacion.Application;
using GmintaDocs.Organizacion.Domain;
using GmintaDocs.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace GmintaDocs.Organizacion.Infrastructure;

public sealed class RepositorioDeEmpresas : RepositorioEfBase<Empresa, long>, IRepositorioDeEmpresas
{
    public RepositorioDeEmpresas(OrganizacionDbContext contexto) : base(contexto) { }

    public Task<Empresa?> ObtenerPorNitAsync(string nit, CancellationToken cancellationToken = default)
        => Conjunto.FirstOrDefaultAsync(e => e.Nit == nit, cancellationToken);

    public Task<ResultadoPaginado<Empresa>> ListarPaginadoAsync(ParametrosDePaginacion parametros, CancellationToken cancellationToken = default)
        => Conjunto.AsNoTracking().OrderBy(e => e.RazonSocial).PaginarAsync(parametros, cancellationToken);
}

public sealed class RepositorioDeSucursales : RepositorioEfBase<Sucursal, long>, IRepositorioDeSucursales
{
    public RepositorioDeSucursales(OrganizacionDbContext contexto) : base(contexto) { }

    public async Task<IReadOnlyList<Sucursal>> ListarPorEmpresaAsync(long idEmpresa, CancellationToken cancellationToken = default)
        => await Conjunto.AsNoTracking()
            .Where(s => s.IdEmpresa == idEmpresa)
            .OrderBy(s => s.Nombre)
            .ToListAsync(cancellationToken);
}
