using GmintaDocs.CQRS;
using GmintaDocs.Organizacion.Domain;
using GmintaDocs.SharedKernel;

namespace GmintaDocs.Organizacion.Application;

public sealed record CrearSucursal(long IdEmpresa, string Codigo, string Nombre, string Direccion,
    string Telefono, string Usuario, string Host) : IComando<Result<long>>;

public sealed record ListarSucursalesDeEmpresa(long IdEmpresa) : IConsulta<IReadOnlyList<SucursalDto>>;

public sealed class CrearSucursalManejador : IManejadorDeComando<CrearSucursal, Result<long>>
{
    private readonly IRepositorioDeSucursales _sucursales;
    private readonly IRepositorioDeEmpresas _empresas;
    private readonly IUnidadDeTrabajoOrganizacion _unidadDeTrabajo;

    public CrearSucursalManejador(IRepositorioDeSucursales sucursales, IRepositorioDeEmpresas empresas,
        IUnidadDeTrabajoOrganizacion unidadDeTrabajo)
    {
        _sucursales = sucursales;
        _empresas = empresas;
        _unidadDeTrabajo = unidadDeTrabajo;
    }

    public async Task<Result<long>> ManejarAsync(CrearSucursal comando, CancellationToken cancellationToken)
    {
        var empresa = await _empresas.ObtenerPorIdAsync(comando.IdEmpresa, cancellationToken);
        if (empresa is null)
            return Result<long>.Fallido($"No existe la empresa {comando.IdEmpresa}.");

        var creacion = Sucursal.Crear(comando.IdEmpresa, comando.Codigo, comando.Nombre,
            comando.Direccion, comando.Telefono, comando.Usuario, comando.Host);
        if (!creacion.EsExitoso)
            return Result<long>.Fallido(creacion.Error!);

        var sucursal = creacion.Valor;
        await _sucursales.AgregarAsync(sucursal, cancellationToken);
        await _unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        return Result<long>.Exitoso(sucursal.Id);
    }
}

public sealed class ListarSucursalesDeEmpresaManejador
    : IManejadorDeConsulta<ListarSucursalesDeEmpresa, IReadOnlyList<SucursalDto>>
{
    private readonly IRepositorioDeSucursales _sucursales;

    public ListarSucursalesDeEmpresaManejador(IRepositorioDeSucursales sucursales) => _sucursales = sucursales;

    public async Task<IReadOnlyList<SucursalDto>> ManejarAsync(ListarSucursalesDeEmpresa consulta,
        CancellationToken cancellationToken)
    {
        var sucursales = await _sucursales.ListarPorEmpresaAsync(consulta.IdEmpresa, cancellationToken);
        return sucursales.Select(s => s.ADto()).ToList();
    }
}
