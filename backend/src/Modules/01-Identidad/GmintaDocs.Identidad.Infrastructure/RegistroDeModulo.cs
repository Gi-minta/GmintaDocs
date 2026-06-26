using GmintaDocs.CQRS;
using GmintaDocs.Identidad.Application;
using GmintaDocs.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GmintaDocs.Identidad.Infrastructure;

public static class RegistroDeModulo
{
    /// <summary>Registra el módulo de Identidad contra la base de datos MAESTRA.</summary>
    public static IServiceCollection AgregarModuloIdentidad(this IServiceCollection servicios, string cadenaMaestra)
    {
        servicios.AddDbContext<IdentidadDbContext>(opciones => opciones.UseNpgsql(cadenaMaestra));
        servicios.AddScoped<IUnidadDeTrabajoIdentidad>(sp => sp.GetRequiredService<IdentidadDbContext>());

        servicios.AddScoped<IRepositorioDeUsuarios, RepositorioDeUsuarios>();
        servicios.AddScoped<IRepositorioDeRoles, RepositorioDeRoles>();
        servicios.AddSingleton<IHasheadorDeContrasena, HasheadorDeContrasenaPbkdf2>();

        servicios.AddScoped<IManejadorDeComando<CrearUsuario, Result<string>>, CrearUsuarioManejador>();
        servicios.AddScoped<IManejadorDeComando<AsignarRolAUsuario, Result>, AsignarRolAUsuarioManejador>();
        servicios.AddScoped<IManejadorDeComando<DesactivarUsuario, Result>, DesactivarUsuarioManejador>();
        servicios.AddScoped<IManejadorDeComando<CambiarContrasena, Result>, CambiarContrasenaManejador>();
        servicios.AddScoped<IManejadorDeComando<CrearRol, Result<string>>, CrearRolManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ObtenerUsuarioPorId, UsuarioDto?>, ObtenerUsuarioPorIdManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ListarUsuarios, ResultadoPaginado<UsuarioDto>>, ListarUsuariosManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ListarRoles, ResultadoPaginado<RolDto>>, ListarRolesManejador>();
        servicios.AddScoped<IManejadorDeConsulta<AutenticarUsuario, Result<UsuarioAutenticadoDto>>, AutenticarUsuarioManejador>();

        return servicios;
    }
}
