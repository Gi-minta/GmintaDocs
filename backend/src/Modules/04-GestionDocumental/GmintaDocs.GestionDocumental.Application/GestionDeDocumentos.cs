using GmintaDocs.CQRS;
using GmintaDocs.GestionDocumental.Domain;
using GmintaDocs.SharedKernel;

namespace GmintaDocs.GestionDocumental.Application;

// ---- Comandos ----
public sealed record CrearTipoDocumento(string Codigo, string Nombre, bool ControlaVersion,
    int DiasVigencia, bool ControlaVigencia, string Usuario, string Host) : IComando<Result<long>>;

public sealed record ActualizarTipoDocumento(long Id, string Nombre, bool ControlaVersion,
    int DiasVigencia, bool ControlaVigencia) : IComando<Result>;
public sealed record EliminarTipoDocumento(long Id) : IComando<Result>;

public sealed record RegistrarArchivo(string Nombre, string Extension, string Directorio, long IdTipoDocumento,
    long Bytes, string? Version, string? Descripcion, string Usuario, string Host) : IComando<Result<long>>;
public sealed record EliminarArchivo(long Id) : IComando<Result>;

// ---- Consultas ----
public sealed record ListarTiposDocumento(ParametrosDePaginacion Parametros) : IConsulta<ResultadoPaginado<TipoDocumentoDto>>;
public sealed record ObtenerArchivoPorId(long Id) : IConsulta<ArchivoDto?>;
public sealed record ListarArchivosPorDirectorio(string Directorio) : IConsulta<IReadOnlyList<ArchivoDto>>;

// ---- Manejadores ----
public sealed class CrearTipoDocumentoManejador : IManejadorDeComando<CrearTipoDocumento, Result<long>>
{
    private readonly IRepositorioDeTiposDocumento _tipos;
    private readonly IUnidadDeTrabajoGestionDocumental _uow;

    public CrearTipoDocumentoManejador(IRepositorioDeTiposDocumento tipos, IUnidadDeTrabajoGestionDocumental uow)
    {
        _tipos = tipos;
        _uow = uow;
    }

    public async Task<Result<long>> ManejarAsync(CrearTipoDocumento comando, CancellationToken cancellationToken)
    {
        var existente = await _tipos.ObtenerPorCodigoAsync(comando.Codigo, cancellationToken);
        if (existente is not null)
            return Result<long>.Fallido($"Ya existe un tipo documental con el código '{comando.Codigo}'.");

        var creacion = TipoDocumento.Crear(comando.Codigo, comando.Nombre, comando.ControlaVersion,
            comando.DiasVigencia, comando.ControlaVigencia, comando.Usuario, comando.Host);
        if (!creacion.EsExitoso)
            return Result<long>.Fallido(creacion.Error!);

        await _tipos.AgregarAsync(creacion.Valor, cancellationToken);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result<long>.Exitoso(creacion.Valor.Id);
    }
}

public sealed class ActualizarTipoDocumentoManejador : IManejadorDeComando<ActualizarTipoDocumento, Result>
{
    private readonly IRepositorioDeTiposDocumento _tipos;
    private readonly IUnidadDeTrabajoGestionDocumental _uow;

    public ActualizarTipoDocumentoManejador(IRepositorioDeTiposDocumento tipos, IUnidadDeTrabajoGestionDocumental uow)
    {
        _tipos = tipos; _uow = uow;
    }

    public async Task<Result> ManejarAsync(ActualizarTipoDocumento comando, CancellationToken cancellationToken)
    {
        var tipo = await _tipos.ObtenerPorIdAsync(comando.Id, cancellationToken);
        if (tipo is null)
            return Result.Fallido($"No existe el tipo documental {comando.Id}.");

        var resultado = tipo.Editar(comando.Nombre, comando.ControlaVersion, comando.DiasVigencia, comando.ControlaVigencia);
        if (!resultado.EsExitoso)
            return resultado;

        _tipos.Actualizar(tipo);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result.Exitoso();
    }
}

public sealed class EliminarTipoDocumentoManejador : IManejadorDeComando<EliminarTipoDocumento, Result>
{
    private readonly IRepositorioDeTiposDocumento _tipos;
    private readonly IUnidadDeTrabajoGestionDocumental _uow;

    public EliminarTipoDocumentoManejador(IRepositorioDeTiposDocumento tipos, IUnidadDeTrabajoGestionDocumental uow)
    {
        _tipos = tipos; _uow = uow;
    }

    public async Task<Result> ManejarAsync(EliminarTipoDocumento comando, CancellationToken cancellationToken)
    {
        var tipo = await _tipos.ObtenerPorIdAsync(comando.Id, cancellationToken);
        if (tipo is null)
            return Result.Fallido($"No existe el tipo documental {comando.Id}.");

        _tipos.Eliminar(tipo);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result.Exitoso();
    }
}

public sealed class EliminarArchivoManejador : IManejadorDeComando<EliminarArchivo, Result>
{
    private readonly IRepositorioDeArchivos _archivos;
    private readonly IUnidadDeTrabajoGestionDocumental _uow;

    public EliminarArchivoManejador(IRepositorioDeArchivos archivos, IUnidadDeTrabajoGestionDocumental uow)
    {
        _archivos = archivos; _uow = uow;
    }

    public async Task<Result> ManejarAsync(EliminarArchivo comando, CancellationToken cancellationToken)
    {
        var archivo = await _archivos.ObtenerPorIdAsync(comando.Id, cancellationToken);
        if (archivo is null)
            return Result.Fallido($"No existe el archivo {comando.Id}.");

        _archivos.Eliminar(archivo);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result.Exitoso();
    }
}

public sealed class RegistrarArchivoManejador : IManejadorDeComando<RegistrarArchivo, Result<long>>
{
    private readonly IRepositorioDeArchivos _archivos;
    private readonly IRepositorioDeTiposDocumento _tipos;
    private readonly IUnidadDeTrabajoGestionDocumental _uow;

    public RegistrarArchivoManejador(IRepositorioDeArchivos archivos, IRepositorioDeTiposDocumento tipos,
        IUnidadDeTrabajoGestionDocumental uow)
    {
        _archivos = archivos;
        _tipos = tipos;
        _uow = uow;
    }

    public async Task<Result<long>> ManejarAsync(RegistrarArchivo comando, CancellationToken cancellationToken)
    {
        var tipo = await _tipos.ObtenerPorIdAsync(comando.IdTipoDocumento, cancellationToken);
        if (tipo is null)
            return Result<long>.Fallido($"No existe el tipo documental {comando.IdTipoDocumento}.");

        var creacion = Archivo.Registrar(comando.Nombre, comando.Extension, comando.Directorio,
            comando.IdTipoDocumento, comando.Bytes, comando.Version, comando.Descripcion, comando.Usuario, comando.Host);
        if (!creacion.EsExitoso)
            return Result<long>.Fallido(creacion.Error!);

        await _archivos.AgregarAsync(creacion.Valor, cancellationToken);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result<long>.Exitoso(creacion.Valor.Id);
    }
}

public sealed class ListarTiposDocumentoManejador : IManejadorDeConsulta<ListarTiposDocumento, ResultadoPaginado<TipoDocumentoDto>>
{
    private readonly IRepositorioDeTiposDocumento _tipos;
    public ListarTiposDocumentoManejador(IRepositorioDeTiposDocumento tipos) => _tipos = tipos;

    public async Task<ResultadoPaginado<TipoDocumentoDto>> ManejarAsync(ListarTiposDocumento consulta, CancellationToken cancellationToken)
    {
        var pagina = await _tipos.ListarPaginadoAsync(consulta.Parametros, cancellationToken);
        return pagina.Mapear(t => t.ADto());
    }
}

public sealed class ObtenerArchivoPorIdManejador : IManejadorDeConsulta<ObtenerArchivoPorId, ArchivoDto?>
{
    private readonly IRepositorioDeArchivos _archivos;
    public ObtenerArchivoPorIdManejador(IRepositorioDeArchivos archivos) => _archivos = archivos;

    public async Task<ArchivoDto?> ManejarAsync(ObtenerArchivoPorId consulta, CancellationToken cancellationToken)
    {
        var archivo = await _archivos.ObtenerPorIdAsync(consulta.Id, cancellationToken);
        return archivo?.ADto();
    }
}

public sealed class ListarArchivosPorDirectorioManejador
    : IManejadorDeConsulta<ListarArchivosPorDirectorio, IReadOnlyList<ArchivoDto>>
{
    private readonly IRepositorioDeArchivos _archivos;
    public ListarArchivosPorDirectorioManejador(IRepositorioDeArchivos archivos) => _archivos = archivos;

    public async Task<IReadOnlyList<ArchivoDto>> ManejarAsync(ListarArchivosPorDirectorio consulta, CancellationToken cancellationToken)
    {
        var archivos = await _archivos.ListarPorDirectorioAsync(consulta.Directorio, cancellationToken);
        return archivos.Select(a => a.ADto()).ToList();
    }
}
