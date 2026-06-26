using GmintaDocs.CQRS;
using GmintaDocs.SharedKernel;
using GmintaDocs.Workflow.Domain;
using InstanciaWorkflow = GmintaDocs.Workflow.Domain.Workflow;

namespace GmintaDocs.Workflow.Application;

// ---- Comandos ----
public sealed record CrearProceso(string Nombre, string? Descripcion, long IdFormulario, string? Version,
    string Usuario, string Host) : IComando<Result<int>>;

public sealed record ActualizarProceso(int Id, string Nombre, string? Descripcion) : IComando<Result>;
public sealed record EliminarProceso(int Id) : IComando<Result>;

public sealed record AgregarPaso(int IdProceso, int Numero, string Descripcion, string? Prioridad,
    int Plazo, string? UnidadPlazo, string Usuario, string Host) : IComando<Result<int>>;

public sealed record IniciarWorkflow(int IdProceso, long IdFormulario, long IdRegistro, string Usuario, string Host)
    : IComando<Result<long>>;

// ---- Consultas ----
public sealed record ObtenerProcesoPorId(int Id) : IConsulta<ProcesoDto?>;
public sealed record ListarProcesos(ParametrosDePaginacion Parametros) : IConsulta<ResultadoPaginado<ProcesoDto>>;
public sealed record ListarPasosDeProceso(int IdProceso) : IConsulta<IReadOnlyList<PasoDto>>;
public sealed record ObtenerWorkflowPorId(long Id) : IConsulta<WorkflowDto?>;

// ---- Manejadores ----
public sealed class CrearProcesoManejador : IManejadorDeComando<CrearProceso, Result<int>>
{
    private readonly IRepositorioDeProcesos _procesos;
    private readonly IUnidadDeTrabajoWorkflow _uow;

    public CrearProcesoManejador(IRepositorioDeProcesos procesos, IUnidadDeTrabajoWorkflow uow)
    {
        _procesos = procesos;
        _uow = uow;
    }

    public async Task<Result<int>> ManejarAsync(CrearProceso comando, CancellationToken cancellationToken)
    {
        var creacion = Proceso.Crear(comando.Nombre, comando.Descripcion, comando.IdFormulario,
            comando.Version, comando.Usuario, comando.Host);
        if (!creacion.EsExitoso)
            return Result<int>.Fallido(creacion.Error!);

        await _procesos.AgregarAsync(creacion.Valor, cancellationToken);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result<int>.Exitoso(creacion.Valor.Id);
    }
}

public sealed class ActualizarProcesoManejador : IManejadorDeComando<ActualizarProceso, Result>
{
    private readonly IRepositorioDeProcesos _procesos;
    private readonly IUnidadDeTrabajoWorkflow _uow;

    public ActualizarProcesoManejador(IRepositorioDeProcesos procesos, IUnidadDeTrabajoWorkflow uow)
    {
        _procesos = procesos; _uow = uow;
    }

    public async Task<Result> ManejarAsync(ActualizarProceso comando, CancellationToken cancellationToken)
    {
        var proceso = await _procesos.ObtenerPorIdAsync(comando.Id, cancellationToken);
        if (proceso is null)
            return Result.Fallido($"No existe el proceso {comando.Id}.");

        var resultado = proceso.Editar(comando.Nombre, comando.Descripcion);
        if (!resultado.EsExitoso)
            return resultado;

        _procesos.Actualizar(proceso);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result.Exitoso();
    }
}

public sealed class EliminarProcesoManejador : IManejadorDeComando<EliminarProceso, Result>
{
    private readonly IRepositorioDeProcesos _procesos;
    private readonly IUnidadDeTrabajoWorkflow _uow;

    public EliminarProcesoManejador(IRepositorioDeProcesos procesos, IUnidadDeTrabajoWorkflow uow)
    {
        _procesos = procesos; _uow = uow;
    }

    public async Task<Result> ManejarAsync(EliminarProceso comando, CancellationToken cancellationToken)
    {
        var proceso = await _procesos.ObtenerPorIdAsync(comando.Id, cancellationToken);
        if (proceso is null)
            return Result.Fallido($"No existe el proceso {comando.Id}.");

        _procesos.Eliminar(proceso);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result.Exitoso();
    }
}

public sealed class AgregarPasoManejador : IManejadorDeComando<AgregarPaso, Result<int>>
{
    private readonly IRepositorioDePasos _pasos;
    private readonly IRepositorioDeProcesos _procesos;
    private readonly IUnidadDeTrabajoWorkflow _uow;

    public AgregarPasoManejador(IRepositorioDePasos pasos, IRepositorioDeProcesos procesos, IUnidadDeTrabajoWorkflow uow)
    {
        _pasos = pasos;
        _procesos = procesos;
        _uow = uow;
    }

    public async Task<Result<int>> ManejarAsync(AgregarPaso comando, CancellationToken cancellationToken)
    {
        var proceso = await _procesos.ObtenerPorIdAsync(comando.IdProceso, cancellationToken);
        if (proceso is null)
            return Result<int>.Fallido($"No existe el proceso {comando.IdProceso}.");

        var creacion = Paso.Crear(comando.IdProceso, comando.Numero, comando.Descripcion, comando.Prioridad,
            comando.Plazo, comando.UnidadPlazo, comando.Usuario, comando.Host);
        if (!creacion.EsExitoso)
            return Result<int>.Fallido(creacion.Error!);

        await _pasos.AgregarAsync(creacion.Valor, cancellationToken);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result<int>.Exitoso(creacion.Valor.Id);
    }
}

public sealed class IniciarWorkflowManejador : IManejadorDeComando<IniciarWorkflow, Result<long>>
{
    private readonly IRepositorioDeWorkflows _workflows;
    private readonly IRepositorioDeProcesos _procesos;
    private readonly IUnidadDeTrabajoWorkflow _uow;

    public IniciarWorkflowManejador(IRepositorioDeWorkflows workflows, IRepositorioDeProcesos procesos, IUnidadDeTrabajoWorkflow uow)
    {
        _workflows = workflows;
        _procesos = procesos;
        _uow = uow;
    }

    public async Task<Result<long>> ManejarAsync(IniciarWorkflow comando, CancellationToken cancellationToken)
    {
        var proceso = await _procesos.ObtenerPorIdAsync(comando.IdProceso, cancellationToken);
        if (proceso is null)
            return Result<long>.Fallido($"No existe el proceso {comando.IdProceso}.");

        var inicio = InstanciaWorkflow.Iniciar(comando.IdProceso, comando.IdFormulario, comando.IdRegistro,
            comando.Usuario, comando.Host);
        if (!inicio.EsExitoso)
            return Result<long>.Fallido(inicio.Error!);

        await _workflows.AgregarAsync(inicio.Valor, cancellationToken);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result<long>.Exitoso(inicio.Valor.Id);
    }
}

public sealed class ObtenerProcesoPorIdManejador : IManejadorDeConsulta<ObtenerProcesoPorId, ProcesoDto?>
{
    private readonly IRepositorioDeProcesos _procesos;
    public ObtenerProcesoPorIdManejador(IRepositorioDeProcesos procesos) => _procesos = procesos;

    public async Task<ProcesoDto?> ManejarAsync(ObtenerProcesoPorId consulta, CancellationToken cancellationToken)
    {
        var proceso = await _procesos.ObtenerPorIdAsync(consulta.Id, cancellationToken);
        return proceso?.ADto();
    }
}

public sealed class ListarProcesosManejador : IManejadorDeConsulta<ListarProcesos, ResultadoPaginado<ProcesoDto>>
{
    private readonly IRepositorioDeProcesos _procesos;
    public ListarProcesosManejador(IRepositorioDeProcesos procesos) => _procesos = procesos;

    public async Task<ResultadoPaginado<ProcesoDto>> ManejarAsync(ListarProcesos consulta, CancellationToken cancellationToken)
    {
        var pagina = await _procesos.ListarPaginadoAsync(consulta.Parametros, cancellationToken);
        return pagina.Mapear(p => p.ADto());
    }
}

public sealed class ListarPasosDeProcesoManejador : IManejadorDeConsulta<ListarPasosDeProceso, IReadOnlyList<PasoDto>>
{
    private readonly IRepositorioDePasos _pasos;
    public ListarPasosDeProcesoManejador(IRepositorioDePasos pasos) => _pasos = pasos;

    public async Task<IReadOnlyList<PasoDto>> ManejarAsync(ListarPasosDeProceso consulta, CancellationToken cancellationToken)
    {
        var pasos = await _pasos.ListarPorProcesoAsync(consulta.IdProceso, cancellationToken);
        return pasos.Select(p => p.ADto()).ToList();
    }
}

public sealed class ObtenerWorkflowPorIdManejador : IManejadorDeConsulta<ObtenerWorkflowPorId, WorkflowDto?>
{
    private readonly IRepositorioDeWorkflows _workflows;
    public ObtenerWorkflowPorIdManejador(IRepositorioDeWorkflows workflows) => _workflows = workflows;

    public async Task<WorkflowDto?> ManejarAsync(ObtenerWorkflowPorId consulta, CancellationToken cancellationToken)
    {
        var workflow = await _workflows.ObtenerPorIdAsync(consulta.Id, cancellationToken);
        return workflow?.ADto();
    }
}
