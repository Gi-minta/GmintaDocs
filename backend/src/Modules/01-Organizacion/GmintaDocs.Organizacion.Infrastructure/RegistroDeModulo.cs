using GmintaDocs.CQRS;
using GmintaDocs.Organizacion.Application;
using GmintaDocs.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GmintaDocs.Organizacion.Infrastructure;

public static class RegistroDeModulo
{
    /// <summary>
    /// Registra el módulo de Organización (núcleo multi-tenant) contra la base de datos MAESTRA.
    /// </summary>
    public static IServiceCollection AgregarModuloOrganizacion(this IServiceCollection servicios, string cadenaMaestra)
    {
        servicios.AddDbContext<OrganizacionDbContext>(opciones => opciones.UseNpgsql(cadenaMaestra));
        servicios.AddScoped<IUnidadDeTrabajoOrganizacion>(sp => sp.GetRequiredService<OrganizacionDbContext>());

        servicios.AddScoped<IRepositorioDeEmpresas, RepositorioDeEmpresas>();
        servicios.AddScoped<IRepositorioDeSucursales, RepositorioDeSucursales>();

        servicios.AddScoped<IManejadorDeComando<CrearEmpresa, Result<long>>, CrearEmpresaManejador>();
        servicios.AddScoped<IManejadorDeComando<ActualizarEmpresa, Result>, ActualizarEmpresaManejador>();
        servicios.AddScoped<IManejadorDeComando<CrearSucursal, Result<long>>, CrearSucursalManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ObtenerEmpresaPorId, EmpresaDto?>, ObtenerEmpresaPorIdManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ListarEmpresas, ResultadoPaginado<EmpresaDto>>, ListarEmpresasManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ListarSucursalesDeEmpresa, IReadOnlyList<SucursalDto>>, ListarSucursalesDeEmpresaManejador>();

        return servicios;
    }
}
