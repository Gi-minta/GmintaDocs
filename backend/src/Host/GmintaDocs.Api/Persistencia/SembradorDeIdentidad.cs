using GmintaDocs.Api.Seguridad;
using GmintaDocs.Identidad.Application;
using GmintaDocs.Identidad.Domain;
using GmintaDocs.Identidad.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace GmintaDocs.Api.Persistencia;

/// <summary>
/// Siembra datos mínimos en la base MAESTRA: los roles del sistema y un usuario administrador
/// inicial. Es idempotente —puede ejecutarse en cada arranque sin duplicar registros— y resuelve
/// el bootstrap (permite el primer acceso sin endpoints anónimos de creación de usuarios).
/// </summary>
public static class SembradorDeIdentidad
{
    public static async Task SembrarIdentidadAsync(this IServiceProvider servicios, IConfiguration configuracion, CancellationToken ct = default)
    {
        await using var scope = servicios.CreateAsyncScope();
        var sp = scope.ServiceProvider;
        var contexto = sp.GetRequiredService<IdentidadDbContext>();
        var hasheador = sp.GetRequiredService<IHasheadorDeContrasena>();
        var registro = sp.GetRequiredService<ILoggerFactory>().CreateLogger("SembradorDeIdentidad");

        // 1) Roles del sistema (Id == Nombre para que coincida con [Authorize(Roles = ...)]).
        foreach (var nombre in RolesDelSistema.Todos)
        {
            if (await contexto.Roles.AnyAsync(r => r.Id == nombre, ct))
                continue;

            var rol = Rol.Crear(nombre, id: nombre);
            if (rol.EsExitoso)
                await contexto.Roles.AddAsync(rol.Valor, ct);
        }
        await contexto.GuardarCambiosAsync(ct);

        // 2) Usuario administrador inicial.
        var opciones = new OpcionesDeAdmin();
        configuracion.GetSection(OpcionesDeAdmin.Seccion).Bind(opciones);

        if (string.IsNullOrWhiteSpace(opciones.Contrasena))
        {
            registro.LogWarning(
                "No se sembró el administrador inicial: falta '{Seccion}:Contrasena' en la configuración.",
                OpcionesDeAdmin.Seccion);
            return;
        }

        if (await contexto.Usuarios.AnyAsync(u => u.UserName == opciones.UserName, ct))
            return;

        var creacion = Usuario.Crear(opciones.UserName, opciones.FullName, opciones.Email);
        if (!creacion.EsExitoso)
        {
            registro.LogWarning("No se pudo crear el administrador inicial: {Error}", creacion.Error);
            return;
        }

        var admin = creacion.Valor;
        admin.EstablecerPasswordHash(hasheador.Hashear(opciones.Contrasena));
        admin.AsignarRol(RolesDelSistema.Administrador);

        await contexto.Usuarios.AddAsync(admin, ct);
        await contexto.GuardarCambiosAsync(ct);
        registro.LogInformation("Administrador inicial '{UserName}' sembrado en la base maestra.", opciones.UserName);
    }
}
