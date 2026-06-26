using GmintaDocs.SharedKernel;
using GmintaDocs.Workflow.Domain;
using InstanciaWorkflow = GmintaDocs.Workflow.Domain.Workflow;

namespace GmintaDocs.Workflow.Application;

/// <summary>Unidad de trabajo del módulo (evita colisión de DI entre los DbContext por módulo).</summary>
public interface IUnidadDeTrabajoWorkflow : IUnidadDeTrabajo { }

public interface IRepositorioDeProcesos : IRepositorio<Proceso, int>
{
    Task<ResultadoPaginado<Proceso>> ListarPaginadoAsync(ParametrosDePaginacion parametros, CancellationToken cancellationToken = default);
}

public interface IRepositorioDePasos : IRepositorio<Paso, int>
{
    Task<IReadOnlyList<Paso>> ListarPorProcesoAsync(int idProceso, CancellationToken cancellationToken = default);
}

public interface IRepositorioDeWorkflows : IRepositorio<InstanciaWorkflow, long>
{
    Task<IReadOnlyList<InstanciaWorkflow>> ListarPorProcesoAsync(int idProceso, CancellationToken cancellationToken = default);
}

public sealed record ProcesoDto(int Id, string Nombre, string Descripcion, long IdFormulario, short Estado, string Version);
public sealed record PasoDto(int Id, int IdProceso, int Numero, string Descripcion, string Prioridad, int Plazo, string UnidadPlazo);
public sealed record WorkflowDto(long Id, int IdProceso, short Estado, long IdFormulario, long IdRegistro, DateTime? FechaFinalizacion);

/// <summary>Mapeos explícitos dominio → DTO (sin AutoMapper, por decisión de stack).</summary>
public static class MapeosWorkflow
{
    public static ProcesoDto ADto(this Proceso p) =>
        new(p.Id, p.Nombre, p.Descripcion, p.IdFormulario, p.Estado, p.Version);

    public static PasoDto ADto(this Paso p) =>
        new(p.Id, p.IdProceso, p.Numero, p.Descripcion, p.Prioridad, p.Plazo, p.UnidadPlazo);

    public static WorkflowDto ADto(this InstanciaWorkflow w) =>
        new(w.Id, w.IdProceso, w.Estado, w.IdFormulario, w.IdRegistro, w.FechaFinalizacion);
}
