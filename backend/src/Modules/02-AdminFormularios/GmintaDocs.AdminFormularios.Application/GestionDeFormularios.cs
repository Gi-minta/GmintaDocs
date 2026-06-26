using GmintaDocs.AdminFormularios.Domain;
using GmintaDocs.CQRS;
using GmintaDocs.SharedKernel;

namespace GmintaDocs.AdminFormularios.Application;

// ---- Comandos ----
public sealed record CrearFormulario(string Codigo, string Tabla, string Nombre, string Usuario, string Host)
    : IComando<Result<long>>;

public sealed record ActualizarFormulario(long Id, string Nombre, string? Descripcion) : IComando<Result>;
public sealed record EliminarFormulario(long Id) : IComando<Result>;

public sealed record AgregarCampo(long IdFormulario, int Orden, string Nombre, string Columna,
    short TipoDato, int LongDato, short Control, bool Requerido, string Usuario, string Host) : IComando<Result<long>>;

// ---- Consultas ----
public sealed record ObtenerFormularioPorId(long Id) : IConsulta<FormularioDto?>;
public sealed record ListarFormularios(ParametrosDePaginacion Parametros) : IConsulta<ResultadoPaginado<FormularioDto>>;
public sealed record ListarCamposDeFormulario(long IdFormulario) : IConsulta<IReadOnlyList<CampoDto>>;

// ---- Manejadores ----
public sealed class CrearFormularioManejador : IManejadorDeComando<CrearFormulario, Result<long>>
{
    private readonly IRepositorioDeFormularios _formularios;
    private readonly IUnidadDeTrabajoAdminFormularios _uow;

    public CrearFormularioManejador(IRepositorioDeFormularios formularios, IUnidadDeTrabajoAdminFormularios uow)
    {
        _formularios = formularios;
        _uow = uow;
    }

    public async Task<Result<long>> ManejarAsync(CrearFormulario comando, CancellationToken cancellationToken)
    {
        var existente = await _formularios.ObtenerPorCodigoAsync(comando.Codigo, cancellationToken);
        if (existente is not null)
            return Result<long>.Fallido($"Ya existe un formulario con el código '{comando.Codigo}'.");

        var creacion = Formulario.Crear(comando.Codigo, comando.Tabla, comando.Nombre, comando.Usuario, comando.Host);
        if (!creacion.EsExitoso)
            return Result<long>.Fallido(creacion.Error!);

        await _formularios.AgregarAsync(creacion.Valor, cancellationToken);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result<long>.Exitoso(creacion.Valor.Id);
    }
}

public sealed class ActualizarFormularioManejador : IManejadorDeComando<ActualizarFormulario, Result>
{
    private readonly IRepositorioDeFormularios _formularios;
    private readonly IUnidadDeTrabajoAdminFormularios _uow;

    public ActualizarFormularioManejador(IRepositorioDeFormularios formularios, IUnidadDeTrabajoAdminFormularios uow)
    {
        _formularios = formularios; _uow = uow;
    }

    public async Task<Result> ManejarAsync(ActualizarFormulario comando, CancellationToken cancellationToken)
    {
        var formulario = await _formularios.ObtenerPorIdAsync(comando.Id, cancellationToken);
        if (formulario is null)
            return Result.Fallido($"No existe el formulario {comando.Id}.");

        var resultado = formulario.Editar(comando.Nombre, comando.Descripcion);
        if (!resultado.EsExitoso)
            return resultado;

        _formularios.Actualizar(formulario);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result.Exitoso();
    }
}

public sealed class EliminarFormularioManejador : IManejadorDeComando<EliminarFormulario, Result>
{
    private readonly IRepositorioDeFormularios _formularios;
    private readonly IUnidadDeTrabajoAdminFormularios _uow;

    public EliminarFormularioManejador(IRepositorioDeFormularios formularios, IUnidadDeTrabajoAdminFormularios uow)
    {
        _formularios = formularios; _uow = uow;
    }

    public async Task<Result> ManejarAsync(EliminarFormulario comando, CancellationToken cancellationToken)
    {
        var formulario = await _formularios.ObtenerPorIdAsync(comando.Id, cancellationToken);
        if (formulario is null)
            return Result.Fallido($"No existe el formulario {comando.Id}.");

        _formularios.Eliminar(formulario);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result.Exitoso();
    }
}

public sealed class AgregarCampoManejador : IManejadorDeComando<AgregarCampo, Result<long>>
{
    private readonly IRepositorioDeCampos _campos;
    private readonly IRepositorioDeFormularios _formularios;
    private readonly IUnidadDeTrabajoAdminFormularios _uow;

    public AgregarCampoManejador(IRepositorioDeCampos campos, IRepositorioDeFormularios formularios,
        IUnidadDeTrabajoAdminFormularios uow)
    {
        _campos = campos;
        _formularios = formularios;
        _uow = uow;
    }

    public async Task<Result<long>> ManejarAsync(AgregarCampo comando, CancellationToken cancellationToken)
    {
        var formulario = await _formularios.ObtenerPorIdAsync(comando.IdFormulario, cancellationToken);
        if (formulario is null)
            return Result<long>.Fallido($"No existe el formulario {comando.IdFormulario}.");

        var creacion = Campo.Crear(comando.IdFormulario, comando.Orden, comando.Nombre, comando.Columna,
            comando.TipoDato, comando.LongDato, comando.Control, comando.Requerido, comando.Usuario, comando.Host);
        if (!creacion.EsExitoso)
            return Result<long>.Fallido(creacion.Error!);

        await _campos.AgregarAsync(creacion.Valor, cancellationToken);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result<long>.Exitoso(creacion.Valor.Id);
    }
}

public sealed class ObtenerFormularioPorIdManejador : IManejadorDeConsulta<ObtenerFormularioPorId, FormularioDto?>
{
    private readonly IRepositorioDeFormularios _formularios;
    public ObtenerFormularioPorIdManejador(IRepositorioDeFormularios formularios) => _formularios = formularios;

    public async Task<FormularioDto?> ManejarAsync(ObtenerFormularioPorId consulta, CancellationToken cancellationToken)
    {
        var formulario = await _formularios.ObtenerPorIdAsync(consulta.Id, cancellationToken);
        return formulario?.ADto();
    }
}

public sealed class ListarFormulariosManejador : IManejadorDeConsulta<ListarFormularios, ResultadoPaginado<FormularioDto>>
{
    private readonly IRepositorioDeFormularios _formularios;
    public ListarFormulariosManejador(IRepositorioDeFormularios formularios) => _formularios = formularios;

    public async Task<ResultadoPaginado<FormularioDto>> ManejarAsync(ListarFormularios consulta, CancellationToken cancellationToken)
    {
        var pagina = await _formularios.ListarPaginadoAsync(consulta.Parametros, cancellationToken);
        return pagina.Mapear(f => f.ADto());
    }
}

public sealed class ListarCamposDeFormularioManejador
    : IManejadorDeConsulta<ListarCamposDeFormulario, IReadOnlyList<CampoDto>>
{
    private readonly IRepositorioDeCampos _campos;
    public ListarCamposDeFormularioManejador(IRepositorioDeCampos campos) => _campos = campos;

    public async Task<IReadOnlyList<CampoDto>> ManejarAsync(ListarCamposDeFormulario consulta, CancellationToken cancellationToken)
    {
        var campos = await _campos.ListarPorFormularioAsync(consulta.IdFormulario, cancellationToken);
        return campos.Select(c => c.ADto()).ToList();
    }
}
