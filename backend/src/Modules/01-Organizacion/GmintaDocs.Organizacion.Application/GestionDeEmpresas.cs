using GmintaDocs.CQRS;
using GmintaDocs.Organizacion.Domain;
using GmintaDocs.SharedKernel;

namespace GmintaDocs.Organizacion.Application;

// ---- Comandos ----

public sealed record CrearEmpresa(string RazonSocial, string Nit, string Usuario, string Host)
    : IComando<Result<long>>;

public sealed record ActualizarEmpresa(long Id, string RazonSocial, string? Direccion, string? Ciudad,
    string? Url, string? Email, string? Telefono, string? Notas) : IComando<Result>;

// ---- Consultas ----

public sealed record ObtenerEmpresaPorId(long Id) : IConsulta<EmpresaDto?>;

public sealed record ListarEmpresas(ParametrosDePaginacion Parametros) : IConsulta<ResultadoPaginado<EmpresaDto>>;

// ---- Manejadores ----

public sealed class CrearEmpresaManejador : IManejadorDeComando<CrearEmpresa, Result<long>>
{
    private readonly IRepositorioDeEmpresas _repositorio;
    private readonly IUnidadDeTrabajoOrganizacion _unidadDeTrabajo;

    public CrearEmpresaManejador(IRepositorioDeEmpresas repositorio, IUnidadDeTrabajoOrganizacion unidadDeTrabajo)
    {
        _repositorio = repositorio;
        _unidadDeTrabajo = unidadDeTrabajo;
    }

    public async Task<Result<long>> ManejarAsync(CrearEmpresa comando, CancellationToken cancellationToken)
    {
        var existente = await _repositorio.ObtenerPorNitAsync(comando.Nit, cancellationToken);
        if (existente is not null)
            return Result<long>.Fallido($"Ya existe una empresa con el NIT {comando.Nit}.");

        var creacion = Empresa.Crear(comando.RazonSocial, comando.Nit, comando.Usuario, comando.Host);
        if (!creacion.EsExitoso)
            return Result<long>.Fallido(creacion.Error!);

        var empresa = creacion.Valor;
        await _repositorio.AgregarAsync(empresa, cancellationToken);
        await _unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        return Result<long>.Exitoso(empresa.Id);
    }
}

public sealed class ActualizarEmpresaManejador : IManejadorDeComando<ActualizarEmpresa, Result>
{
    private readonly IRepositorioDeEmpresas _repositorio;
    private readonly IUnidadDeTrabajoOrganizacion _unidadDeTrabajo;

    public ActualizarEmpresaManejador(IRepositorioDeEmpresas repositorio, IUnidadDeTrabajoOrganizacion unidadDeTrabajo)
    {
        _repositorio = repositorio;
        _unidadDeTrabajo = unidadDeTrabajo;
    }

    public async Task<Result> ManejarAsync(ActualizarEmpresa comando, CancellationToken cancellationToken)
    {
        var empresa = await _repositorio.ObtenerPorIdAsync(comando.Id, cancellationToken);
        if (empresa is null)
            return Result.Fallido($"No existe la empresa {comando.Id}.");

        var resultado = empresa.ActualizarDatos(comando.RazonSocial, comando.Direccion, comando.Ciudad,
            comando.Url, comando.Email, comando.Telefono, comando.Notas);
        if (!resultado.EsExitoso)
            return resultado;

        _repositorio.Actualizar(empresa);
        await _unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        return Result.Exitoso();
    }
}

public sealed class ObtenerEmpresaPorIdManejador : IManejadorDeConsulta<ObtenerEmpresaPorId, EmpresaDto?>
{
    private readonly IRepositorioDeEmpresas _repositorio;

    public ObtenerEmpresaPorIdManejador(IRepositorioDeEmpresas repositorio) => _repositorio = repositorio;

    public async Task<EmpresaDto?> ManejarAsync(ObtenerEmpresaPorId consulta, CancellationToken cancellationToken)
    {
        var empresa = await _repositorio.ObtenerPorIdAsync(consulta.Id, cancellationToken);
        return empresa?.ADto();
    }
}

public sealed class ListarEmpresasManejador : IManejadorDeConsulta<ListarEmpresas, ResultadoPaginado<EmpresaDto>>
{
    private readonly IRepositorioDeEmpresas _repositorio;

    public ListarEmpresasManejador(IRepositorioDeEmpresas repositorio) => _repositorio = repositorio;

    public async Task<ResultadoPaginado<EmpresaDto>> ManejarAsync(ListarEmpresas consulta, CancellationToken cancellationToken)
    {
        var pagina = await _repositorio.ListarPaginadoAsync(consulta.Parametros, cancellationToken);
        return pagina.Mapear(e => e.ADto());
    }
}
