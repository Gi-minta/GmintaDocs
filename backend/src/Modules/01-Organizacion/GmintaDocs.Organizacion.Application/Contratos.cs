using GmintaDocs.Organizacion.Domain;
using GmintaDocs.SharedKernel;

namespace GmintaDocs.Organizacion.Application;

/// <summary>Unidad de trabajo específica del módulo (evita colisión de DI entre los DbContext de cada módulo).</summary>
public interface IUnidadDeTrabajoOrganizacion : IUnidadDeTrabajo { }

public interface IRepositorioDeEmpresas : IRepositorio<Empresa, long>
{
    Task<Empresa?> ObtenerPorNitAsync(string nit, CancellationToken cancellationToken = default);
    Task<ResultadoPaginado<Empresa>> ListarPaginadoAsync(ParametrosDePaginacion parametros, CancellationToken cancellationToken = default);
}

public interface IRepositorioDeSucursales : IRepositorio<Sucursal, long>
{
    Task<IReadOnlyList<Sucursal>> ListarPorEmpresaAsync(long idEmpresa, CancellationToken cancellationToken = default);
}

/// <summary>DTO de lectura para Empresa.</summary>
public sealed record EmpresaDto(
    long Id, string RazonSocial, string Nit, string? Direccion, string? Ciudad,
    string? Url, string? Email, string? Telefono, string? Notas);

/// <summary>DTO de lectura para Sucursal.</summary>
public sealed record SucursalDto(
    long Id, long IdEmpresa, string Codigo, string Nombre, string Direccion,
    string Telefono, bool Activa);

/// <summary>Mapeos explícitos dominio → DTO (sin AutoMapper, por decisión de stack).</summary>
public static class MapeosOrganizacion
{
    public static EmpresaDto ADto(this Empresa e) =>
        new(e.Id, e.RazonSocial, e.Nit, e.Direccion, e.Ciudad, e.Url, e.Email, e.Telefono, e.Notas);

    public static SucursalDto ADto(this Sucursal s) =>
        new(s.Id, s.IdEmpresa, s.Codigo, s.Nombre, s.Direccion, s.Telefono, s.Activa);
}
