using GmintaDocs.SharedKernel;

namespace GmintaDocs.GestionDocumental.Domain;

/// <summary>Documento/archivo gestionado, con control de versiones (tabla archivos).</summary>
public sealed class Archivo : AggregateRoot<long>
{
    public string Nombre { get; private set; } = string.Empty; // columna "archivo"
    public string Extension { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public DateTime FechaDocumento { get; private set; }
    public DateTime FechaPublicacion { get; private set; }
    public string Directorio { get; private set; } = string.Empty;
    public short Estado { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public string Host { get; private set; } = string.Empty;
    public long IdPrimeraVersion { get; private set; }
    public bool EsVersionActual { get; private set; }
    public string Version { get; private set; } = string.Empty;
    public string? Observacion { get; private set; }
    public long IdArchivoPrincipal { get; private set; }
    public long IdTipoDocumento { get; private set; }
    public long Bytes { get; private set; }
    public string? Etiquetas { get; private set; }
    public string? UnidadConservacion { get; private set; }
    public string? UnidadAlmacenamiento { get; private set; }
    public long? IdFormularioV10 { get; private set; }
    public long? IdRegistroV10 { get; private set; }
    public long? IdRegistroArchivoV10 { get; private set; }
    public int? IdDirectorioV10 { get; private set; }
    public string? RutaV10 { get; private set; }

    private Archivo() { }

    /// <summary>Registra un documento como su primera versión vigente dentro de un directorio.</summary>
    public static Result<Archivo> Registrar(string nombre, string extension, string directorio,
        long idTipoDocumento, long bytes, string? version, string? descripcion, string usuario, string host)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return Result<Archivo>.Fallido("El nombre del archivo es obligatorio.");
        if (idTipoDocumento <= 0) return Result<Archivo>.Fallido("El tipo documental es obligatorio.");

        var ahora = DateTime.UtcNow;
        return Result<Archivo>.Exitoso(new Archivo
        {
            Nombre = nombre.Trim(), Extension = extension?.Trim() ?? string.Empty,
            Directorio = directorio?.Trim() ?? string.Empty, Descripcion = descripcion,
            Estado = 1, FechaDocumento = ahora, FechaPublicacion = ahora, IdPrimeraVersion = 0,
            EsVersionActual = true, Version = string.IsNullOrWhiteSpace(version) ? "1" : version,
            IdArchivoPrincipal = 0, IdTipoDocumento = idTipoDocumento, Bytes = bytes,
            Usuario = usuario, Host = host
        });
    }
}

/// <summary>Relación de un archivo con un registro/formulario/workflow (tabla archivos_formulario).</summary>
public sealed class ArchivoFormulario : AggregateRoot<long>
{
    public long IdArchivo { get; private set; }
    public long IdRegistro { get; private set; }
    public long IdFormulario { get; private set; }
    public long? IdDirectorio { get; private set; }
    public long? IdWorkflow { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public string Host { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }

    private ArchivoFormulario() { }
}

/// <summary>Relación entre archivos (tabla relacion_archivos).</summary>
public sealed class RelacionArchivo : AggregateRoot<long>
{
    public long IdArchivo { get; private set; }
    public long IdRelacion { get; private set; }

    private RelacionArchivo() { }
}

/// <summary>Proyección desnormalizada para búsqueda de archivos (tabla busqueda_archivos, solo lectura).</summary>
public sealed class BusquedaArchivo
{
    public long IdArchivo { get; private set; }
    public string Archivo { get; private set; } = string.Empty;
    public string Extension { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public DateTime FechaDocumento { get; private set; }
    public DateTime FechaPublicacion { get; private set; }
    public string Directorio { get; private set; } = string.Empty;
    public short Estado { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public string Host { get; private set; } = string.Empty;
    public long IdPrimeraVersion { get; private set; }
    public bool EsVersionActual { get; private set; }
    public string Version { get; private set; } = string.Empty;
    public string? Observacion { get; private set; }
    public long IdArchivoPrincipal { get; private set; }
    public long IdTipoDocumento { get; private set; }
    public long Bytes { get; private set; }
    public string? Etiquetas { get; private set; }
    public long? Id { get; private set; }
    public long? IdRegistro { get; private set; }
    public long? IdFormulario { get; private set; }
    public long? IdDirectorio { get; private set; }
    public long? IdWorkflow { get; private set; }
}

/// <summary>Tipo documental (tabla tipo_documento).</summary>
public sealed class TipoDocumento : AggregateRoot<long>
{
    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public bool ControlaVersion { get; private set; }
    public int DiasVigencia { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;
    public bool ControlaVigencia { get; private set; }

    private TipoDocumento() { }

    public static Result<TipoDocumento> Crear(string codigo, string nombre, bool controlaVersion,
        int diasVigencia, bool controlaVigencia, string usuario, string host)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return Result<TipoDocumento>.Fallido("El código es obligatorio.");
        if (string.IsNullOrWhiteSpace(nombre)) return Result<TipoDocumento>.Fallido("El nombre es obligatorio.");

        return Result<TipoDocumento>.Exitoso(new TipoDocumento
        {
            Codigo = codigo.Trim(), Nombre = nombre.Trim(), ControlaVersion = controlaVersion,
            DiasVigencia = diasVigencia, ControlaVigencia = controlaVigencia, Usuario = usuario,
            Host = host, Fecha = DateTime.UtcNow
        });
    }

    public Result Editar(string nombre, bool controlaVersion, int diasVigencia, bool controlaVigencia)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return Result.Fallido("El nombre es obligatorio.");
        Nombre = nombre.Trim();
        ControlaVersion = controlaVersion;
        DiasVigencia = diasVigencia;
        ControlaVigencia = controlaVigencia;
        return Result.Exitoso();
    }
}

/// <summary>Ayuda al almacenar: sugiere tipo documental/directorio por formulario (tabla ayuda_almacenar).</summary>
public sealed class AyudaAlmacenar : AggregateRoot<long>
{
    public long IdTipoDocumento { get; private set; }
    public long IdDirectorio { get; private set; }
    public long IdFormulario { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;

    private AyudaAlmacenar() { }
}

/// <summary>Auditoría de eventos sobre registros de negocio (tabla eventos).</summary>
public sealed class Evento : AggregateRoot<long>
{
    public string Tabla { get; private set; } = string.Empty;
    public long IdRegistro { get; private set; }
    public string Accion { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public string Host { get; private set; } = string.Empty;
    public string Comentario { get; private set; } = string.Empty;

    private Evento() { }
}

/// <summary>Tabla de Retención Documental — TRD (tabla trd). Regla de negocio 5.x del SGDEA.</summary>
public sealed class Trd : AggregateRoot<long>
{
    public long? IdTipoDocumental { get; private set; }
    public float? RetencionAg { get; private set; }
    public float? RetencionAc { get; private set; }
    public bool ConservacionTotal { get; private set; }
    public bool Eliminacion { get; private set; }
    public bool Microfilmacion { get; private set; }
    public bool Seleccion { get; private set; }
    public float? PorcentajeSeleccion { get; private set; }
    public bool Facilitativo { get; private set; }
    public bool Facultativo { get; private set; }
    public bool Sustantivo { get; private set; }
    public bool Legal { get; private set; }
    public bool Fiscal { get; private set; }
    public bool Contable { get; private set; }
    public bool Funcional { get; private set; }
    public bool Administrativo { get; private set; }
    public bool Historico { get; private set; }
    public bool Cientifico { get; private set; }
    public bool Cultural { get; private set; }
    public bool Misional { get; private set; }
    public string? Procedimiento { get; private set; }
    public string? Normatividad { get; private set; }
    public string? Observaciones { get; private set; }
    public bool Ley1581 { get; private set; }
    public float? RetencionElectronica { get; private set; }
    public string? EliminacionElectronica { get; private set; }
    public bool Fisico { get; private set; }
    public bool Inmaterializado { get; private set; }
    public bool Desmaterializado { get; private set; }
    public bool Simple { get; private set; }
    public bool Integro { get; private set; }
    public bool Autentico { get; private set; }
    public bool FirmaDigital { get; private set; }
    public bool FirmaBiometrica { get; private set; }
    public bool EstampadoCronologico { get; private set; }
    public string? Seguridad { get; private set; }
    public string? NivelSeguridad { get; private set; }
    public bool PerteneceSgc { get; private set; }
    public long IdFormulario { get; private set; }
    public long IdRegistro { get; private set; }
    public bool EvidenciaNiif { get; private set; }

    private Trd() { }
}
