using GmintaDocs.AdminDirectorios.Infrastructure;
using GmintaDocs.AdminFormularios.Infrastructure;
using GmintaDocs.Colaboracion.Infrastructure;
using GmintaDocs.GestionDocumental.Infrastructure;
using GmintaDocs.Identidad.Infrastructure;
using GmintaDocs.Multitenancy;
using GmintaDocs.Organizacion.Infrastructure;
using GmintaDocs.Plantillas.Infrastructure;
using GmintaDocs.Reportes.Infrastructure;
using GmintaDocs.Tareas.Infrastructure;
using GmintaDocs.Workflow.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace GmintaDocs.Api.Persistencia;

/// <summary>
/// Aplica migraciones EF. La base MAESTRA (Identidad + Organización) se migra al arrancar;
/// las bases por empresa se migran de forma explícita al aprovisionar la empresa.
/// </summary>
public static class MigradorDeBaseDeDatos
{
    /// <summary>Migra la base de datos de control (maestra) que aloja Identidad y Organización.</summary>
    public static async Task AplicarMigracionesMaestrasAsync(this IServiceProvider servicios, CancellationToken ct = default)
    {
        await using var scope = servicios.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IdentidadDbContext>().Database.MigrateAsync(ct);
        await scope.ServiceProvider.GetRequiredService<OrganizacionDbContext>().Database.MigrateAsync(ct);
    }
}

/// <summary>
/// Crea (si no existe) y migra la base de datos dedicada de una empresa, aplicando las migraciones
/// de los ocho módulos de negocio contra la cadena de conexión resuelta para esa empresa.
/// </summary>
public sealed class AprovisionadorDeEmpresa
{
    private readonly IServiceProvider _servicios;
    private readonly IResolvedorDeTenant _resolvedor;

    public AprovisionadorDeEmpresa(IServiceProvider servicios, IResolvedorDeTenant resolvedor)
    {
        _servicios = servicios;
        _resolvedor = resolvedor;
    }

    public async Task AprovisionarAsync(long idEmpresa, CancellationToken ct = default)
    {
        await using var scope = _servicios.CreateAsyncScope();

        // Fija la empresa activa en el ámbito para que cada DbContext por tenant resuelva su cadena.
        var tenant = scope.ServiceProvider.GetRequiredService<ContextoDeTenant>();
        tenant.Establecer(idEmpresa, $"Empresa {idEmpresa}", _resolvedor.ResolverCadenaDeConexion(idEmpresa), null);

        await MigrarAsync<AdminFormulariosDbContext>(scope.ServiceProvider, ct);
        await MigrarAsync<AdminDirectoriosDbContext>(scope.ServiceProvider, ct);
        await MigrarAsync<WorkflowDbContext>(scope.ServiceProvider, ct);
        await MigrarAsync<TareasDbContext>(scope.ServiceProvider, ct);
        await MigrarAsync<GestionDocumentalDbContext>(scope.ServiceProvider, ct);
        await MigrarAsync<PlantillasDbContext>(scope.ServiceProvider, ct);
        await MigrarAsync<ReportesDbContext>(scope.ServiceProvider, ct);
        await MigrarAsync<ColaboracionDbContext>(scope.ServiceProvider, ct);
    }

    private static Task MigrarAsync<TContext>(IServiceProvider sp, CancellationToken ct) where TContext : DbContext
        => sp.GetRequiredService<TContext>().Database.MigrateAsync(ct);
}
