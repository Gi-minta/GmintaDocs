using GmintaDocs.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace GmintaDocs.Multitenancy;

/// <summary>
/// Extensiones de paginación sobre <see cref="IQueryable{T}"/> para EF Core: cuentan el total y
/// traen solo la página solicitada (OFFSET/LIMIT en la base de datos, no en memoria).
/// </summary>
public static class PaginacionEf
{
    public static async Task<ResultadoPaginado<T>> PaginarAsync<T>(
        this IQueryable<T> consulta, ParametrosDePaginacion parametros, CancellationToken cancellationToken = default)
    {
        var total = await consulta.CountAsync(cancellationToken);
        var elementos = await consulta
            .Skip(parametros.Salto)
            .Take(parametros.TamanoNormalizado)
            .ToListAsync(cancellationToken);

        return new ResultadoPaginado<T>(elementos, parametros.PaginaNormalizada, parametros.TamanoNormalizado, total);
    }
}
