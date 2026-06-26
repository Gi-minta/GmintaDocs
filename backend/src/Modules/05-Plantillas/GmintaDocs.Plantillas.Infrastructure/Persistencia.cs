using GmintaDocs.CQRS;
using GmintaDocs.Multitenancy;
using GmintaDocs.Plantillas.Application;
using GmintaDocs.Plantillas.Domain;
using GmintaDocs.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GmintaDocs.Plantillas.Infrastructure;

/// <summary>Contexto del módulo de plantillas (base de datos por empresa).</summary>
public sealed class PlantillasDbContext : DbContext, IUnidadDeTrabajoPlantillas
{
    public PlantillasDbContext(DbContextOptions<PlantillasDbContext> options) : base(options) { }

    public DbSet<Plantilla> Plantillas => Set<Plantilla>();
    public DbSet<PlantillaFormato> PlantillasFormato => Set<PlantillaFormato>();
    public DbSet<ParametroFormato> ParametrosFormato => Set<ParametroFormato>();

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Plantilla>(e =>
        {
            e.ToTable("plantillas");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
            e.Property(x => x.Contenido).HasColumnName("plantilla");
        });
        modelBuilder.Entity<PlantillaFormato>(e =>
        {
            e.ToTable("plantillas_formato");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
        });
        modelBuilder.Entity<ParametroFormato>(e =>
        {
            e.ToTable("parametros_formato");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id_parametro").UseIdentityAlwaysColumn();
        });

        ConvencionesDeNomenclatura.AplicarSnakeCaseAColumnas(modelBuilder);
    }
}

public static class RegistroDeModulo
{
    public static IServiceCollection AgregarModuloPlantillas(this IServiceCollection servicios)
    {
        servicios.AgregarDbContextDeTenant<PlantillasDbContext>();
        servicios.AddScoped<IUnidadDeTrabajoPlantillas>(sp => sp.GetRequiredService<PlantillasDbContext>());

        servicios.AddScoped<IRepositorioDePlantillas, RepositorioDePlantillas>();
        servicios.AddScoped<IRepositorioDePlantillasFormato, RepositorioDePlantillasFormato>();

        servicios.AddScoped<IManejadorDeComando<CrearPlantilla, Result<int>>, CrearPlantillaManejador>();
        servicios.AddScoped<IManejadorDeComando<ActualizarPlantilla, Result>, ActualizarPlantillaManejador>();
        servicios.AddScoped<IManejadorDeComando<EliminarPlantilla, Result>, EliminarPlantillaManejador>();
        servicios.AddScoped<IManejadorDeComando<CrearPlantillaFormato, Result<int>>, CrearPlantillaFormatoManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ObtenerPlantillaPorId, PlantillaDto?>, ObtenerPlantillaPorIdManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ListarPlantillas, ResultadoPaginado<PlantillaDto>>, ListarPlantillasManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ListarPlantillasFormato, IReadOnlyList<PlantillaFormatoDto>>, ListarPlantillasFormatoManejador>();

        return servicios;
    }
}
