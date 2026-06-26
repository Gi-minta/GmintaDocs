using GmintaDocs.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace GmintaDocs.Multitenancy;

/// <summary>
/// Implementación base de <see cref="IRepositorio{TAggregate,TId}"/> sobre EF Core.
/// Los repositorios concretos de cada módulo heredan de esta clase y añaden sus consultas específicas.
/// </summary>
public abstract class RepositorioEfBase<TAggregate, TId> : IRepositorio<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
    protected DbContext Contexto { get; }
    protected DbSet<TAggregate> Conjunto { get; }

    protected RepositorioEfBase(DbContext contexto)
    {
        Contexto = contexto;
        Conjunto = contexto.Set<TAggregate>();
    }

    public virtual async Task<TAggregate?> ObtenerPorIdAsync(TId id, CancellationToken cancellationToken = default)
        => await Conjunto.FindAsync([id], cancellationToken);

    public virtual async Task AgregarAsync(TAggregate agregado, CancellationToken cancellationToken = default)
        => await Conjunto.AddAsync(agregado, cancellationToken);

    public virtual void Actualizar(TAggregate agregado) => Conjunto.Update(agregado);

    public virtual void Eliminar(TAggregate agregado) => Conjunto.Remove(agregado);
}
