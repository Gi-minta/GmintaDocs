namespace GmintaDocs.SharedKernel;

/// <summary>
/// Repositorio genérico para una raíz de agregado. Las consultas específicas
/// se definen en interfaces especializadas por agregado dentro de cada módulo.
/// </summary>
public interface IRepositorio<TAggregate, in TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
    Task<TAggregate?> ObtenerPorIdAsync(TId id, CancellationToken cancellationToken = default);
    Task AgregarAsync(TAggregate agregado, CancellationToken cancellationToken = default);
    void Actualizar(TAggregate agregado);
    void Eliminar(TAggregate agregado);
}

/// <summary>
/// Confirma los cambios pendientes de un contexto de persistencia como una unidad atómica.
/// </summary>
public interface IUnidadDeTrabajo
{
    Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
