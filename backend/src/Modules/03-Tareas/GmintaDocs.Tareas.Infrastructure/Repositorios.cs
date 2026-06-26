using GmintaDocs.Multitenancy;
using GmintaDocs.Tareas.Application;
using GmintaDocs.Tareas.Domain;
using Microsoft.EntityFrameworkCore;

namespace GmintaDocs.Tareas.Infrastructure;

public sealed class RepositorioDeTareas : RepositorioEfBase<Tarea, long>, IRepositorioDeTareas
{
    public RepositorioDeTareas(TareasDbContext contexto) : base(contexto) { }

    public async Task<IReadOnlyList<Tarea>> ListarPorResponsableAsync(string responsable, CancellationToken cancellationToken = default)
        => await Conjunto.AsNoTracking()
            .Where(t => t.Responsable == responsable)
            .OrderByDescending(t => t.FechaVencimiento)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Tarea>> ListarPorWorkflowAsync(long idWorkflow, CancellationToken cancellationToken = default)
        => await Conjunto.AsNoTracking()
            .Where(t => t.IdWorkflow == idWorkflow)
            .OrderBy(t => t.Paso)
            .ToListAsync(cancellationToken);
}
