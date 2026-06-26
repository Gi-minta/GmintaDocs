using GmintaDocs.AdminDirectorios.Application;
using GmintaDocs.AdminDirectorios.Domain;
using GmintaDocs.CQRS;
using GmintaDocs.Multitenancy;
using GmintaDocs.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GmintaDocs.AdminDirectorios.Infrastructure;

/// <summary>Contexto del módulo de administración de directorios (base de datos por empresa).</summary>
public sealed class AdminDirectoriosDbContext : DbContext, IUnidadDeTrabajoAdminDirectorios
{
    public AdminDirectoriosDbContext(DbContextOptions<AdminDirectoriosDbContext> options) : base(options) { }

    public DbSet<Directorio> Directorios => Set<Directorio>();

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Directorio>(e =>
        {
            e.ToTable("directorios");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id_directorio").UseIdentityAlwaysColumn();
        });

        ConvencionesDeNomenclatura.AplicarSnakeCaseAColumnas(modelBuilder);
    }
}

public static class RegistroDeModulo
{
    public static IServiceCollection AgregarModuloAdminDirectorios(this IServiceCollection servicios)
    {
        servicios.AgregarDbContextDeTenant<AdminDirectoriosDbContext>();
        servicios.AddScoped<IUnidadDeTrabajoAdminDirectorios>(sp => sp.GetRequiredService<AdminDirectoriosDbContext>());

        servicios.AddScoped<IRepositorioDeDirectorios, RepositorioDeDirectorios>();

        servicios.AddScoped<IManejadorDeComando<CrearDirectorio, Result<long>>, CrearDirectorioManejador>();
        servicios.AddScoped<IManejadorDeComando<ActualizarDirectorio, Result>, ActualizarDirectorioManejador>();
        servicios.AddScoped<IManejadorDeComando<EliminarDirectorio, Result>, EliminarDirectorioManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ObtenerDirectorioPorId, DirectorioDto?>, ObtenerDirectorioPorIdManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ListarDirectoriosDeFormulario, IReadOnlyList<DirectorioDto>>, ListarDirectoriosDeFormularioManejador>();

        return servicios;
    }
}
