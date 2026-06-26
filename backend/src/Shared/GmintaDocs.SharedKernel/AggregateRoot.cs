namespace GmintaDocs.SharedKernel;

/// <summary>
/// Marca una entidad como raíz de agregado dentro de un contexto delimitado (Bounded Context).
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    protected AggregateRoot() { }
    protected AggregateRoot(TId id) : base(id) { }
}
