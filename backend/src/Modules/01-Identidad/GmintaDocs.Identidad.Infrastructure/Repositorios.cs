using GmintaDocs.Identidad.Application;
using GmintaDocs.Identidad.Domain;
using GmintaDocs.Multitenancy;
using GmintaDocs.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace GmintaDocs.Identidad.Infrastructure;

public sealed class RepositorioDeUsuarios : RepositorioEfBase<Usuario, string>, IRepositorioDeUsuarios
{
    private readonly IdentidadDbContext _contexto;

    public RepositorioDeUsuarios(IdentidadDbContext contexto) : base(contexto) => _contexto = contexto;

    public override Task<Usuario?> ObtenerPorIdAsync(string id, CancellationToken cancellationToken = default)
        => _contexto.Usuarios.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<Usuario?> ObtenerPorUserNameAsync(string userName, CancellationToken cancellationToken = default)
        => _contexto.Usuarios.Include(u => u.Roles).FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);

    public Task<ResultadoPaginado<Usuario>> ListarPaginadoAsync(ParametrosDePaginacion parametros, CancellationToken cancellationToken = default)
        => _contexto.Usuarios.AsNoTracking().Include(u => u.Roles)
            .OrderBy(u => u.UserName).PaginarAsync(parametros, cancellationToken);
}

public sealed class RepositorioDeRoles : RepositorioEfBase<Rol, string>, IRepositorioDeRoles
{
    public RepositorioDeRoles(IdentidadDbContext contexto) : base(contexto) { }

    public Task<ResultadoPaginado<Rol>> ListarPaginadoAsync(ParametrosDePaginacion parametros, CancellationToken cancellationToken = default)
        => Conjunto.AsNoTracking().OrderBy(r => r.Nombre).PaginarAsync(parametros, cancellationToken);
}
