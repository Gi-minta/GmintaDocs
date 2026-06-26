using GmintaDocs.SharedKernel;
using GmintaDocs.Tareas.Domain;

namespace GmintaDocs.Tareas.Application;

/// <summary>Unidad de trabajo del módulo (evita colisión de DI entre los DbContext por módulo).</summary>
public interface IUnidadDeTrabajoTareas : IUnidadDeTrabajo { }

public interface IRepositorioDeTareas : IRepositorio<Tarea, long>
{
    Task<IReadOnlyList<Tarea>> ListarPorResponsableAsync(string responsable, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tarea>> ListarPorWorkflowAsync(long idWorkflow, CancellationToken cancellationToken = default);
}

public sealed record TareaDto(long Id, long IdWorkflow, string Asunto, short Estado, string Prioridad,
    string Responsable, DateTime FechaVencimiento, DateTime? FechaEjecucion);

/// <summary>Mapeos explícitos dominio → DTO (sin AutoMapper, por decisión de stack).</summary>
public static class MapeosTareas
{
    public static TareaDto ADto(this Tarea t) =>
        new(t.Id, t.IdWorkflow, t.Asunto, t.Estado, t.Prioridad, t.Responsable, t.FechaVencimiento, t.FechaEjecucion);
}
