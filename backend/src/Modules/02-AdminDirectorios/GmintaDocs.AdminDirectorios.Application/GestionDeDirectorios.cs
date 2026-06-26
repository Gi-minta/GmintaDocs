using GmintaDocs.AdminDirectorios.Domain;
using GmintaDocs.CQRS;
using GmintaDocs.SharedKernel;

namespace GmintaDocs.AdminDirectorios.Application;

// ---- Comandos ----
public sealed record CrearDirectorio(long IdFormulario, long IdDirectorioPadre, string Codigo, string Nombre,
    string Usuario, string Host) : IComando<Result<long>>;

public sealed record ActualizarDirectorio(long Id, string Nombre, string Codigo) : IComando<Result>;
public sealed record EliminarDirectorio(long Id) : IComando<Result>;

// ---- Consultas ----
public sealed record ObtenerDirectorioPorId(long Id) : IConsulta<DirectorioDto?>;
public sealed record ListarDirectoriosDeFormulario(long IdFormulario) : IConsulta<IReadOnlyList<DirectorioDto>>;

// ---- Manejadores ----
public sealed class CrearDirectorioManejador : IManejadorDeComando<CrearDirectorio, Result<long>>
{
    private readonly IRepositorioDeDirectorios _directorios;
    private readonly IUnidadDeTrabajoAdminDirectorios _uow;

    public CrearDirectorioManejador(IRepositorioDeDirectorios directorios, IUnidadDeTrabajoAdminDirectorios uow)
    {
        _directorios = directorios;
        _uow = uow;
    }

    public async Task<Result<long>> ManejarAsync(CrearDirectorio comando, CancellationToken cancellationToken)
    {
        var creacion = Directorio.Crear(comando.IdFormulario, comando.IdDirectorioPadre, comando.Codigo,
            comando.Nombre, comando.Usuario, comando.Host);
        if (!creacion.EsExitoso)
            return Result<long>.Fallido(creacion.Error!);

        await _directorios.AgregarAsync(creacion.Valor, cancellationToken);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result<long>.Exitoso(creacion.Valor.Id);
    }
}

public sealed class ActualizarDirectorioManejador : IManejadorDeComando<ActualizarDirectorio, Result>
{
    private readonly IRepositorioDeDirectorios _directorios;
    private readonly IUnidadDeTrabajoAdminDirectorios _uow;

    public ActualizarDirectorioManejador(IRepositorioDeDirectorios directorios, IUnidadDeTrabajoAdminDirectorios uow)
    {
        _directorios = directorios; _uow = uow;
    }

    public async Task<Result> ManejarAsync(ActualizarDirectorio comando, CancellationToken cancellationToken)
    {
        var directorio = await _directorios.ObtenerPorIdAsync(comando.Id, cancellationToken);
        if (directorio is null)
            return Result.Fallido($"No existe el directorio {comando.Id}.");

        var resultado = directorio.Editar(comando.Nombre, comando.Codigo);
        if (!resultado.EsExitoso)
            return resultado;

        _directorios.Actualizar(directorio);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result.Exitoso();
    }
}

public sealed class EliminarDirectorioManejador : IManejadorDeComando<EliminarDirectorio, Result>
{
    private readonly IRepositorioDeDirectorios _directorios;
    private readonly IUnidadDeTrabajoAdminDirectorios _uow;

    public EliminarDirectorioManejador(IRepositorioDeDirectorios directorios, IUnidadDeTrabajoAdminDirectorios uow)
    {
        _directorios = directorios; _uow = uow;
    }

    public async Task<Result> ManejarAsync(EliminarDirectorio comando, CancellationToken cancellationToken)
    {
        var directorio = await _directorios.ObtenerPorIdAsync(comando.Id, cancellationToken);
        if (directorio is null)
            return Result.Fallido($"No existe el directorio {comando.Id}.");

        _directorios.Eliminar(directorio);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result.Exitoso();
    }
}

public sealed class ObtenerDirectorioPorIdManejador : IManejadorDeConsulta<ObtenerDirectorioPorId, DirectorioDto?>
{
    private readonly IRepositorioDeDirectorios _directorios;
    public ObtenerDirectorioPorIdManejador(IRepositorioDeDirectorios directorios) => _directorios = directorios;

    public async Task<DirectorioDto?> ManejarAsync(ObtenerDirectorioPorId consulta, CancellationToken cancellationToken)
    {
        var directorio = await _directorios.ObtenerPorIdAsync(consulta.Id, cancellationToken);
        return directorio?.ADto();
    }
}

public sealed class ListarDirectoriosDeFormularioManejador
    : IManejadorDeConsulta<ListarDirectoriosDeFormulario, IReadOnlyList<DirectorioDto>>
{
    private readonly IRepositorioDeDirectorios _directorios;
    public ListarDirectoriosDeFormularioManejador(IRepositorioDeDirectorios directorios) => _directorios = directorios;

    public async Task<IReadOnlyList<DirectorioDto>> ManejarAsync(ListarDirectoriosDeFormulario consulta, CancellationToken cancellationToken)
    {
        var directorios = await _directorios.ListarPorFormularioAsync(consulta.IdFormulario, cancellationToken);
        return directorios.Select(d => d.ADto()).ToList();
    }
}
