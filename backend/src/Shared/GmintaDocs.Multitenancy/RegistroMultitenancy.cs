using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GmintaDocs.Multitenancy;

public static class RegistroMultitenancy
{
    /// <summary>
    /// Servicios base de multitenancy: contexto de tenant scoped y resolución de cadena de conexión.
    /// </summary>
    public static IServiceCollection AgregarMultitenancy(this IServiceCollection servicios)
    {
        servicios.AddScoped<ContextoDeTenant>();
        servicios.AddScoped<IContextoDeTenant>(sp => sp.GetRequiredService<ContextoDeTenant>());
        servicios.AddSingleton<IResolvedorDeTenant, ResolvedorDeTenantDesdeConfiguracion>();
        return servicios;
    }

    /// <summary>
    /// Registra un <c>DbContext</c> que resuelve su cadena de conexión en runtime
    /// según la empresa (tenant) activa. Para módulos de negocio con BD dedicada por empresa.
    /// </summary>
    public static IServiceCollection AgregarDbContextDeTenant<TContext>(this IServiceCollection servicios)
        where TContext : DbContext
    {
        servicios.AddDbContext<TContext>((sp, opciones) =>
        {
            var tenant = sp.GetRequiredService<IContextoDeTenant>();
            opciones.UseNpgsql(tenant.CadenaDeConexion);
        });
        return servicios;
    }
}
