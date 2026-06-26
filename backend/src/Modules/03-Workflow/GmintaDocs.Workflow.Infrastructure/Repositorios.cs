using GmintaDocs.Multitenancy;
using GmintaDocs.SharedKernel;
using GmintaDocs.Workflow.Application;
using GmintaDocs.Workflow.Domain;
using Microsoft.EntityFrameworkCore;
using InstanciaWorkflow = GmintaDocs.Workflow.Domain.Workflow;

namespace GmintaDocs.Workflow.Infrastructure;

public sealed class RepositorioDeProcesos : RepositorioEfBase<Proceso, int>, IRepositorioDeProcesos
{
    public RepositorioDeProcesos(WorkflowDbContext contexto) : base(contexto) { }

    public Task<ResultadoPaginado<Proceso>> ListarPaginadoAsync(ParametrosDePaginacion parametros, CancellationToken cancellationToken = default)
        => Conjunto.AsNoTracking().OrderBy(p => p.Nombre).PaginarAsync(parametros, cancellationToken);
}

public sealed class RepositorioDePasos : RepositorioEfBase<Paso, int>, IRepositorioDePasos
{
    public RepositorioDePasos(WorkflowDbContext contexto) : base(contexto) { }

    public async Task<IReadOnlyList<Paso>> ListarPorProcesoAsync(int idProceso, CancellationToken cancellationToken = default)
        => await Conjunto.AsNoTracking()
            .Where(p => p.IdProceso == idProceso)
            .OrderBy(p => p.Numero)
            .ToListAsync(cancellationToken);
}

public sealed class RepositorioDeWorkflows : RepositorioEfBase<InstanciaWorkflow, long>, IRepositorioDeWorkflows
{
    public RepositorioDeWorkflows(WorkflowDbContext contexto) : base(contexto) { }

    public async Task<IReadOnlyList<InstanciaWorkflow>> ListarPorProcesoAsync(int idProceso, CancellationToken cancellationToken = default)
        => await Conjunto.AsNoTracking()
            .Where(w => w.IdProceso == idProceso)
            .OrderByDescending(w => w.Fecha)
            .ToListAsync(cancellationToken);
}
