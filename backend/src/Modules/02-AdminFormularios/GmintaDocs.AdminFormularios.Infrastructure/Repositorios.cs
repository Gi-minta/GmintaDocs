using GmintaDocs.AdminFormularios.Application;
using GmintaDocs.AdminFormularios.Domain;
using GmintaDocs.Multitenancy;
using GmintaDocs.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace GmintaDocs.AdminFormularios.Infrastructure;

public sealed class RepositorioDeFormularios : RepositorioEfBase<Formulario, long>, IRepositorioDeFormularios
{
    public RepositorioDeFormularios(AdminFormulariosDbContext contexto) : base(contexto) { }

    public Task<Formulario?> ObtenerPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
        => Conjunto.FirstOrDefaultAsync(f => f.Codigo == codigo, cancellationToken);

    public Task<ResultadoPaginado<Formulario>> ListarPaginadoAsync(ParametrosDePaginacion parametros, CancellationToken cancellationToken = default)
        => Conjunto.AsNoTracking().OrderBy(f => f.Nombre).PaginarAsync(parametros, cancellationToken);
}

public sealed class RepositorioDeCampos : RepositorioEfBase<Campo, long>, IRepositorioDeCampos
{
    public RepositorioDeCampos(AdminFormulariosDbContext contexto) : base(contexto) { }

    public async Task<IReadOnlyList<Campo>> ListarPorFormularioAsync(long idFormulario, CancellationToken cancellationToken = default)
        => await Conjunto.AsNoTracking()
            .Where(c => c.IdFormulario == idFormulario)
            .OrderBy(c => c.Orden)
            .ToListAsync(cancellationToken);
}
