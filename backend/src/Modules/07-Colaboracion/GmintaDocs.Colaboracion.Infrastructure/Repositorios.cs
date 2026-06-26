using GmintaDocs.Colaboracion.Application;
using GmintaDocs.Colaboracion.Domain;
using GmintaDocs.Multitenancy;
using GmintaDocs.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace GmintaDocs.Colaboracion.Infrastructure;

public sealed class RepositorioDeNoticias : RepositorioEfBase<Noticia, long>, IRepositorioDeNoticias
{
    public RepositorioDeNoticias(ColaboracionDbContext contexto) : base(contexto) { }

    public Task<ResultadoPaginado<Noticia>> ListarPaginadoAsync(ParametrosDePaginacion parametros, CancellationToken cancellationToken = default)
        => Conjunto.AsNoTracking().OrderByDescending(n => n.FechaPublicacion).PaginarAsync(parametros, cancellationToken);
}

public sealed class RepositorioDeComentarios : RepositorioEfBase<Comentario, long>, IRepositorioDeComentarios
{
    public RepositorioDeComentarios(ColaboracionDbContext contexto) : base(contexto) { }

    public async Task<IReadOnlyList<Comentario>> ListarPorNoticiaAsync(long idNoticia, CancellationToken cancellationToken = default)
        => await Conjunto.AsNoTracking()
            .Where(c => c.IdNoticia == idNoticia)
            .OrderBy(c => c.FechaPublicacion)
            .ToListAsync(cancellationToken);
}
