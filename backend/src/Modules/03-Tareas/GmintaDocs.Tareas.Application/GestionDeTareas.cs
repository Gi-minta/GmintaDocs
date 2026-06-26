using GmintaDocs.CQRS;
using GmintaDocs.SharedKernel;
using GmintaDocs.Tareas.Domain;

namespace GmintaDocs.Tareas.Application;

// ---- Comandos ----
public sealed record CrearTarea(long IdWorkflow, string Asunto, string? Descripcion, string? Prioridad,
    int Paso, string Responsable, string? Remitente, DateTime FechaVencimiento, string Host) : IComando<Result<long>>;

public sealed record EjecutarTarea(long IdTarea, short Estado) : IComando<Result>;
public sealed record ActualizarTarea(long Id, string Asunto, string? Descripcion, string? Prioridad,
    string Responsable, DateTime FechaVencimiento) : IComando<Result>;
public sealed record EliminarTarea(long Id) : IComando<Result>;

// ---- Consultas ----
public sealed record ObtenerTareaPorId(long Id) : IConsulta<TareaDto?>;
public sealed record ListarTareasPorResponsable(string Responsable) : IConsulta<IReadOnlyList<TareaDto>>;

// ---- Manejadores ----
public sealed class CrearTareaManejador : IManejadorDeComando<CrearTarea, Result<long>>
{
    private readonly IRepositorioDeTareas _tareas;
    private readonly IUnidadDeTrabajoTareas _uow;

    public CrearTareaManejador(IRepositorioDeTareas tareas, IUnidadDeTrabajoTareas uow)
    {
        _tareas = tareas;
        _uow = uow;
    }

    public async Task<Result<long>> ManejarAsync(CrearTarea comando, CancellationToken cancellationToken)
    {
        var creacion = Tarea.Crear(comando.IdWorkflow, comando.Asunto, comando.Descripcion, comando.Prioridad,
            comando.Paso, comando.Responsable, comando.Remitente, comando.FechaVencimiento, comando.Host);
        if (!creacion.EsExitoso)
            return Result<long>.Fallido(creacion.Error!);

        await _tareas.AgregarAsync(creacion.Valor, cancellationToken);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result<long>.Exitoso(creacion.Valor.Id);
    }
}

public sealed class EjecutarTareaManejador : IManejadorDeComando<EjecutarTarea, Result>
{
    private readonly IRepositorioDeTareas _tareas;
    private readonly IUnidadDeTrabajoTareas _uow;

    public EjecutarTareaManejador(IRepositorioDeTareas tareas, IUnidadDeTrabajoTareas uow)
    {
        _tareas = tareas;
        _uow = uow;
    }

    public async Task<Result> ManejarAsync(EjecutarTarea comando, CancellationToken cancellationToken)
    {
        var tarea = await _tareas.ObtenerPorIdAsync(comando.IdTarea, cancellationToken);
        if (tarea is null)
            return Result.Fallido($"No existe la tarea {comando.IdTarea}.");

        tarea.Ejecutar(comando.Estado);
        _tareas.Actualizar(tarea);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result.Exitoso();
    }
}

public sealed class ActualizarTareaManejador : IManejadorDeComando<ActualizarTarea, Result>
{
    private readonly IRepositorioDeTareas _tareas;
    private readonly IUnidadDeTrabajoTareas _uow;

    public ActualizarTareaManejador(IRepositorioDeTareas tareas, IUnidadDeTrabajoTareas uow)
    {
        _tareas = tareas; _uow = uow;
    }

    public async Task<Result> ManejarAsync(ActualizarTarea comando, CancellationToken cancellationToken)
    {
        var tarea = await _tareas.ObtenerPorIdAsync(comando.Id, cancellationToken);
        if (tarea is null)
            return Result.Fallido($"No existe la tarea {comando.Id}.");

        var resultado = tarea.Editar(comando.Asunto, comando.Descripcion, comando.Prioridad,
            comando.Responsable, comando.FechaVencimiento);
        if (!resultado.EsExitoso)
            return resultado;

        _tareas.Actualizar(tarea);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result.Exitoso();
    }
}

public sealed class EliminarTareaManejador : IManejadorDeComando<EliminarTarea, Result>
{
    private readonly IRepositorioDeTareas _tareas;
    private readonly IUnidadDeTrabajoTareas _uow;

    public EliminarTareaManejador(IRepositorioDeTareas tareas, IUnidadDeTrabajoTareas uow)
    {
        _tareas = tareas; _uow = uow;
    }

    public async Task<Result> ManejarAsync(EliminarTarea comando, CancellationToken cancellationToken)
    {
        var tarea = await _tareas.ObtenerPorIdAsync(comando.Id, cancellationToken);
        if (tarea is null)
            return Result.Fallido($"No existe la tarea {comando.Id}.");

        _tareas.Eliminar(tarea);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result.Exitoso();
    }
}

public sealed class ObtenerTareaPorIdManejador : IManejadorDeConsulta<ObtenerTareaPorId, TareaDto?>
{
    private readonly IRepositorioDeTareas _tareas;
    public ObtenerTareaPorIdManejador(IRepositorioDeTareas tareas) => _tareas = tareas;

    public async Task<TareaDto?> ManejarAsync(ObtenerTareaPorId consulta, CancellationToken cancellationToken)
    {
        var tarea = await _tareas.ObtenerPorIdAsync(consulta.Id, cancellationToken);
        return tarea?.ADto();
    }
}

public sealed class ListarTareasPorResponsableManejador
    : IManejadorDeConsulta<ListarTareasPorResponsable, IReadOnlyList<TareaDto>>
{
    private readonly IRepositorioDeTareas _tareas;
    public ListarTareasPorResponsableManejador(IRepositorioDeTareas tareas) => _tareas = tareas;

    public async Task<IReadOnlyList<TareaDto>> ManejarAsync(ListarTareasPorResponsable consulta, CancellationToken cancellationToken)
    {
        var tareas = await _tareas.ListarPorResponsableAsync(consulta.Responsable, cancellationToken);
        return tareas.Select(t => t.ADto()).ToList();
    }
}
