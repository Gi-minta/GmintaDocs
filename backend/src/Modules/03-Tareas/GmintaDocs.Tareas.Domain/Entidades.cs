using GmintaDocs.SharedKernel;

namespace GmintaDocs.Tareas.Domain;

/// <summary>Tarea asignada dentro de un workflow (tabla tareas).</summary>
public sealed class Tarea : AggregateRoot<long>
{
    public long IdWorkflow { get; private set; }
    public string Asunto { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public short Estado { get; private set; }
    public string Prioridad { get; private set; } = string.Empty;
    public int Paso { get; private set; }
    public string Responsable { get; private set; } = string.Empty;
    public string Remitente { get; private set; } = string.Empty;
    public short Tipo { get; private set; }
    public bool Aviso { get; private set; }
    public DateTime FechaAviso { get; private set; }
    public DateTime FechaRecepcion { get; private set; }
    public DateTime FechaVencimiento { get; private set; }
    public DateTime? FechaEjecucion { get; private set; }
    public int Dias { get; private set; }
    public int Horas { get; private set; }
    public int Minutos { get; private set; }
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;
    public int PasoDestino { get; private set; }
    public int PasoSiguiente { get; private set; }
    public string? ResponsablePasoSiguiente { get; private set; }

    private Tarea() { }

    public static Result<Tarea> Crear(long idWorkflow, string asunto, string? descripcion, string? prioridad,
        int paso, string responsable, string? remitente, DateTime fechaVencimiento, string host)
    {
        if (idWorkflow <= 0) return Result<Tarea>.Fallido("La tarea debe pertenecer a un workflow válido.");
        if (string.IsNullOrWhiteSpace(asunto)) return Result<Tarea>.Fallido("El asunto es obligatorio.");
        if (string.IsNullOrWhiteSpace(responsable)) return Result<Tarea>.Fallido("El responsable es obligatorio.");

        var ahora = DateTime.UtcNow;
        return Result<Tarea>.Exitoso(new Tarea
        {
            IdWorkflow = idWorkflow, Asunto = asunto.Trim(), Descripcion = descripcion?.Trim() ?? string.Empty,
            Estado = 1, Prioridad = string.IsNullOrWhiteSpace(prioridad) ? "Normal" : prioridad, Paso = paso,
            Responsable = responsable, Remitente = remitente ?? string.Empty, Tipo = 0, Aviso = false,
            FechaAviso = fechaVencimiento, FechaRecepcion = ahora, FechaVencimiento = fechaVencimiento,
            Host = host, Fecha = ahora
        });
    }

    /// <summary>Regla de negocio: una tarea ejecutada registra su fecha de ejecución y cambia de estado.</summary>
    public void Ejecutar(short estadoEjecutado)
    {
        Estado = estadoEjecutado;
        FechaEjecucion = DateTime.UtcNow;
    }

    public Result Editar(string asunto, string? descripcion, string? prioridad, string responsable, DateTime fechaVencimiento)
    {
        if (string.IsNullOrWhiteSpace(asunto)) return Result.Fallido("El asunto es obligatorio.");
        if (string.IsNullOrWhiteSpace(responsable)) return Result.Fallido("El responsable es obligatorio.");

        Asunto = asunto.Trim();
        Descripcion = descripcion?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(prioridad)) Prioridad = prioridad;
        Responsable = responsable;
        FechaVencimiento = fechaVencimiento;
        return Result.Exitoso();
    }
}

/// <summary>Comentario sobre una tarea (tabla comentarios_tarea).</summary>
public sealed class ComentarioTarea : AggregateRoot<long>
{
    public long IdTarea { get; private set; }
    public long IdWorkflow { get; private set; }
    public string Autor { get; private set; } = string.Empty;
    public string Avatar { get; private set; } = string.Empty;
    public string Texto { get; private set; } = string.Empty;
    public string Usuario { get; private set; } = string.Empty;
    public DateTime FechaPublicacion { get; private set; }
    public string Host { get; private set; } = string.Empty;

    private ComentarioTarea() { }
}

/// <summary>Carpeta organizativa de tareas (tabla carpeta_tarea).</summary>
public sealed class CarpetaTarea : AggregateRoot<long>
{
    public string Nombre { get; private set; } = string.Empty;
    public short Padre { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;

    private CarpetaTarea() { }
}

/// <summary>Contenido (tarea) de una carpeta (tabla contenido_carpeta).</summary>
public sealed class ContenidoCarpeta : AggregateRoot<long>
{
    public long IdTarea { get; private set; }
    public long IdCarpetaTarea { get; private set; }

    private ContenidoCarpeta() { }
}

/// <summary>Lote de tareas de un paso del workflow (tabla lote_wf).</summary>
public sealed class LoteWf : AggregateRoot<long>
{
    public int IdPaso { get; private set; }
    public short Estado { get; private set; }
    public string Responsable { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string Usuario { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;

    private LoteWf() { }
}

/// <summary>Relación entre un lote y una tarea (tabla tareas_lote).</summary>
public sealed class TareaLote : AggregateRoot<long>
{
    public long IdLote { get; private set; }
    public long IdTarea { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;

    private TareaLote() { }
}

/// <summary>Evento de agenda/calendario (tabla agenda).</summary>
public sealed class Agenda : AggregateRoot<long>
{
    public string Titulo { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public DateTime Comienza { get; private set; }
    public DateTime Finaliza { get; private set; }
    public bool? EsTodoDia { get; private set; }
    public string? ReglaRecurrencia { get; private set; }
    public string? ExcepcionRecurrencia { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;

    private Agenda() { }
}
