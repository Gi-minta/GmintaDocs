using GmintaDocs.SharedKernel;

namespace GmintaDocs.Workflow.Domain;

/// <summary>Definición de un proceso de negocio (tabla proceso).</summary>
public sealed class Proceso : AggregateRoot<int>
{
    public string Nombre { get; private set; } = string.Empty; // columna "proceso"
    public string Descripcion { get; private set; } = string.Empty;
    public long IdFormulario { get; private set; }
    public short Estado { get; private set; }
    public string Version { get; private set; } = string.Empty;
    public string Usuario { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;

    private Proceso() { }

    public static Result<Proceso> Crear(string nombre, string? descripcion, long idFormulario,
        string? version, string usuario, string host)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return Result<Proceso>.Fallido("El nombre del proceso es obligatorio.");

        return Result<Proceso>.Exitoso(new Proceso
        {
            Nombre = nombre.Trim(), Descripcion = descripcion?.Trim() ?? string.Empty, IdFormulario = idFormulario,
            Estado = 1, Version = string.IsNullOrWhiteSpace(version) ? "1" : version, Usuario = usuario,
            Host = host, Fecha = DateTime.UtcNow
        });
    }

    public Result Editar(string nombre, string? descripcion)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return Result.Fallido("El nombre del proceso es obligatorio.");
        Nombre = nombre.Trim();
        Descripcion = descripcion?.Trim() ?? string.Empty;
        return Result.Exitoso();
    }
}

/// <summary>Paso de un proceso (tabla paso).</summary>
public sealed class Paso : AggregateRoot<int>
{
    public int IdProceso { get; private set; }
    public int Numero { get; private set; } // columna "paso"
    public string Descripcion { get; private set; } = string.Empty;
    public string Prioridad { get; private set; } = string.Empty;
    public int Plazo { get; private set; }
    public string UnidadPlazo { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public string Host { get; private set; } = string.Empty;

    private Paso() { }

    public static Result<Paso> Crear(int idProceso, int numero, string descripcion, string? prioridad,
        int plazo, string? unidadPlazo, string usuario, string host)
    {
        if (idProceso <= 0) return Result<Paso>.Fallido("El paso debe pertenecer a un proceso válido.");
        if (string.IsNullOrWhiteSpace(descripcion)) return Result<Paso>.Fallido("La descripción del paso es obligatoria.");

        return Result<Paso>.Exitoso(new Paso
        {
            IdProceso = idProceso, Numero = numero, Descripcion = descripcion.Trim(),
            Prioridad = string.IsNullOrWhiteSpace(prioridad) ? "Normal" : prioridad, Plazo = plazo,
            UnidadPlazo = string.IsNullOrWhiteSpace(unidadPlazo) ? "dias" : unidadPlazo,
            Usuario = usuario, Host = host, Fecha = DateTime.UtcNow
        });
    }
}

/// <summary>Valor de configuración de un paso (tabla configuracion_paso).</summary>
public sealed class ConfiguracionPaso : AggregateRoot<long>
{
    public int IdPaso { get; private set; }
    public int IdOpcion { get; private set; }
    public string Valor { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public string Host { get; private set; } = string.Empty;

    private ConfiguracionPaso() { }
}

/// <summary>Paso candidato para una devolución (tabla posibles_pasos_devolucion, clave compuesta).</summary>
public sealed class PosiblePasoDevolucion
{
    public int Paso { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public int IdProceso { get; private set; }

    private PosiblePasoDevolucion() { }
}

/// <summary>Instancia en ejecución de un proceso (tabla workflow).</summary>
public sealed class Workflow : AggregateRoot<long>
{
    public int IdProceso { get; private set; }
    public short Estado { get; private set; }
    public long IdFormulario { get; private set; }
    public long IdRegistro { get; private set; }
    public DateTime? FechaFinalizacion { get; private set; }
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;
    public string Usuario { get; private set; } = string.Empty;
    public long? IdWorkflowV10 { get; private set; }
    public string? WorkflowV10 { get; private set; }

    private Workflow() { }

    /// <summary>Regla de negocio: iniciar un workflow crea una instancia en ejecución (estado 1) de un proceso.</summary>
    public static Result<Workflow> Iniciar(int idProceso, long idFormulario, long idRegistro, string usuario, string host)
    {
        if (idProceso <= 0) return Result<Workflow>.Fallido("El workflow requiere un proceso válido.");

        return Result<Workflow>.Exitoso(new Workflow
        {
            IdProceso = idProceso, Estado = 1, IdFormulario = idFormulario, IdRegistro = idRegistro,
            Usuario = usuario, Host = host, Fecha = DateTime.UtcNow
        });
    }

    /// <summary>Regla de negocio: finalizar marca el estado terminal y registra la fecha de finalización.</summary>
    public void Finalizar()
    {
        Estado = 2;
        FechaFinalizacion = DateTime.UtcNow;
    }
}

/// <summary>Vínculo entre un workflow y un registro de formulario (tabla formulario_workflow).</summary>
public sealed class FormularioWorkflow : AggregateRoot<long>
{
    public long IdWorkflow { get; private set; }
    public long IdFormulario { get; private set; }
    public long IdRegistro { get; private set; }
    public string Tipo { get; private set; } = string.Empty;
    public string Usuario { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;

    private FormularioWorkflow() { }
}

/// <summary>Grupo de trabajo del workflow (tabla grupos_wf).</summary>
public sealed class GrupoWf : AggregateRoot<int>
{
    public string Nombre { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public string Usuario { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;

    private GrupoWf() { }
}

/// <summary>Miembro de un grupo de trabajo (tabla miembros_grupo_wf).</summary>
public sealed class MiembroGrupoWf : AggregateRoot<int>
{
    public int IdGrupo { get; private set; }
    public string Miembro { get; private set; } = string.Empty;
    public string Usuario { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;

    private MiembroGrupoWf() { }
}

/// <summary>Evidencia generada en la ejecución de un paso (tabla evidencias).</summary>
public sealed class Evidencia : AggregateRoot<long>
{
    public int IdOpcion { get; private set; }
    public long IdWorkflow { get; private set; }
    public long IdTarea { get; private set; }
    public string Parametros { get; private set; } = string.Empty;
    public string Estado { get; private set; } = string.Empty;
    public string? IdTransaccion { get; private set; }
    public string? RespuestaJson { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;
    public string? DocumentoHtml { get; private set; }
    public string? NombreEvidencia { get; private set; }
    public bool Procesado { get; private set; }

    private Evidencia() { }
}
