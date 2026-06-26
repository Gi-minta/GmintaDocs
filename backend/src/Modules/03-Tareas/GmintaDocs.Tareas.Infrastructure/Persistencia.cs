using GmintaDocs.CQRS;
using GmintaDocs.Multitenancy;
using GmintaDocs.SharedKernel;
using GmintaDocs.Tareas.Application;
using GmintaDocs.Tareas.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GmintaDocs.Tareas.Infrastructure;

/// <summary>Contexto del módulo de tareas (base de datos por empresa).</summary>
public sealed class TareasDbContext : DbContext, IUnidadDeTrabajoTareas
{
    public TareasDbContext(DbContextOptions<TareasDbContext> options) : base(options) { }

    public DbSet<Tarea> Tareas => Set<Tarea>();
    public DbSet<ComentarioTarea> ComentariosTarea => Set<ComentarioTarea>();
    public DbSet<CarpetaTarea> CarpetasTarea => Set<CarpetaTarea>();
    public DbSet<ContenidoCarpeta> ContenidosCarpeta => Set<ContenidoCarpeta>();
    public DbSet<LoteWf> LotesWf => Set<LoteWf>();
    public DbSet<TareaLote> TareasLote => Set<TareaLote>();
    public DbSet<Agenda> Agendas => Set<Agenda>();

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tarea>(e =>
        {
            e.ToTable("tareas");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id_tarea").UseIdentityAlwaysColumn();
        });
        modelBuilder.Entity<ComentarioTarea>(e =>
        {
            e.ToTable("comentarios_tarea");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
        });
        modelBuilder.Entity<CarpetaTarea>(e =>
        {
            e.ToTable("carpeta_tarea");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
        });
        modelBuilder.Entity<ContenidoCarpeta>(e =>
        {
            e.ToTable("contenido_carpeta");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
        });
        modelBuilder.Entity<LoteWf>(e =>
        {
            e.ToTable("lote_wf");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id_lote").UseIdentityAlwaysColumn();
        });
        modelBuilder.Entity<TareaLote>(e =>
        {
            e.ToTable("tareas_lote");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
        });
        modelBuilder.Entity<Agenda>(e =>
        {
            e.ToTable("agenda");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
        });

        ConvencionesDeNomenclatura.AplicarSnakeCaseAColumnas(modelBuilder);
    }
}

public static class RegistroDeModulo
{
    public static IServiceCollection AgregarModuloTareas(this IServiceCollection servicios)
    {
        servicios.AgregarDbContextDeTenant<TareasDbContext>();
        servicios.AddScoped<IUnidadDeTrabajoTareas>(sp => sp.GetRequiredService<TareasDbContext>());

        servicios.AddScoped<IRepositorioDeTareas, RepositorioDeTareas>();

        servicios.AddScoped<IManejadorDeComando<CrearTarea, Result<long>>, CrearTareaManejador>();
        servicios.AddScoped<IManejadorDeComando<EjecutarTarea, Result>, EjecutarTareaManejador>();
        servicios.AddScoped<IManejadorDeComando<ActualizarTarea, Result>, ActualizarTareaManejador>();
        servicios.AddScoped<IManejadorDeComando<EliminarTarea, Result>, EliminarTareaManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ObtenerTareaPorId, TareaDto?>, ObtenerTareaPorIdManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ListarTareasPorResponsable, IReadOnlyList<TareaDto>>, ListarTareasPorResponsableManejador>();

        return servicios;
    }
}
