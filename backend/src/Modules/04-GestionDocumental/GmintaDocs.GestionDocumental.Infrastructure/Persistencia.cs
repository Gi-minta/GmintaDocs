using GmintaDocs.CQRS;
using GmintaDocs.GestionDocumental.Application;
using GmintaDocs.GestionDocumental.Domain;
using GmintaDocs.Multitenancy;
using GmintaDocs.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GmintaDocs.GestionDocumental.Infrastructure;

/// <summary>Contexto del módulo de gestión documental (base de datos por empresa).</summary>
public sealed class GestionDocumentalDbContext : DbContext, IUnidadDeTrabajoGestionDocumental
{
    public GestionDocumentalDbContext(DbContextOptions<GestionDocumentalDbContext> options) : base(options) { }

    public DbSet<Archivo> Archivos => Set<Archivo>();
    public DbSet<ArchivoFormulario> ArchivosFormulario => Set<ArchivoFormulario>();
    public DbSet<RelacionArchivo> RelacionesArchivo => Set<RelacionArchivo>();
    public DbSet<BusquedaArchivo> BusquedaArchivos => Set<BusquedaArchivo>();
    public DbSet<TipoDocumento> TiposDocumento => Set<TipoDocumento>();
    public DbSet<AyudaAlmacenar> AyudasAlmacenar => Set<AyudaAlmacenar>();
    public DbSet<Evento> Eventos => Set<Evento>();
    public DbSet<Trd> Trds => Set<Trd>();

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Archivo>(e =>
        {
            e.ToTable("archivos");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id_archivo").UseIdentityAlwaysColumn();
            e.Property(x => x.Nombre).HasColumnName("archivo");
        });
        modelBuilder.Entity<ArchivoFormulario>(e =>
        {
            e.ToTable("archivos_formulario");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
        });
        modelBuilder.Entity<RelacionArchivo>(e =>
        {
            e.ToTable("relacion_archivos");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
        });
        modelBuilder.Entity<BusquedaArchivo>(e =>
        {
            e.ToTable("busqueda_archivos");
            e.HasNoKey();
            e.Property(x => x.Archivo).HasColumnName("archivo");
        });
        modelBuilder.Entity<TipoDocumento>(e =>
        {
            e.ToTable("tipo_documento");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id_tipo_documento").UseIdentityAlwaysColumn();
        });
        modelBuilder.Entity<AyudaAlmacenar>(e =>
        {
            e.ToTable("ayuda_almacenar");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
        });
        modelBuilder.Entity<Evento>(e =>
        {
            e.ToTable("eventos");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
        });
        modelBuilder.Entity<Trd>(e =>
        {
            e.ToTable("trd");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
        });

        ConvencionesDeNomenclatura.AplicarSnakeCaseAColumnas(modelBuilder);
    }
}

public static class RegistroDeModulo
{
    public static IServiceCollection AgregarModuloGestionDocumental(this IServiceCollection servicios)
    {
        servicios.AgregarDbContextDeTenant<GestionDocumentalDbContext>();
        servicios.AddScoped<IUnidadDeTrabajoGestionDocumental>(sp => sp.GetRequiredService<GestionDocumentalDbContext>());

        servicios.AddScoped<IRepositorioDeArchivos, RepositorioDeArchivos>();
        servicios.AddScoped<IRepositorioDeTiposDocumento, RepositorioDeTiposDocumento>();

        servicios.AddScoped<IManejadorDeComando<CrearTipoDocumento, Result<long>>, CrearTipoDocumentoManejador>();
        servicios.AddScoped<IManejadorDeComando<ActualizarTipoDocumento, Result>, ActualizarTipoDocumentoManejador>();
        servicios.AddScoped<IManejadorDeComando<EliminarTipoDocumento, Result>, EliminarTipoDocumentoManejador>();
        servicios.AddScoped<IManejadorDeComando<RegistrarArchivo, Result<long>>, RegistrarArchivoManejador>();
        servicios.AddScoped<IManejadorDeComando<EliminarArchivo, Result>, EliminarArchivoManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ListarTiposDocumento, ResultadoPaginado<TipoDocumentoDto>>, ListarTiposDocumentoManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ObtenerArchivoPorId, ArchivoDto?>, ObtenerArchivoPorIdManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ListarArchivosPorDirectorio, IReadOnlyList<ArchivoDto>>, ListarArchivosPorDirectorioManejador>();

        return servicios;
    }
}
