using GmintaDocs.CQRS;
using GmintaDocs.Multitenancy;
using GmintaDocs.SharedKernel;
using GmintaDocs.Workflow.Application;
using GmintaDocs.Workflow.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GmintaDocs.Workflow.Infrastructure;

/// <summary>Contexto del módulo de workflow (base de datos por empresa).</summary>
public sealed class WorkflowDbContext : DbContext, IUnidadDeTrabajoWorkflow
{
    public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options) : base(options) { }

    public DbSet<Proceso> Procesos => Set<Proceso>();
    public DbSet<Paso> Pasos => Set<Paso>();
    public DbSet<ConfiguracionPaso> ConfiguracionesPaso => Set<ConfiguracionPaso>();
    public DbSet<PosiblePasoDevolucion> PosiblesPasosDevolucion => Set<PosiblePasoDevolucion>();
    public DbSet<Domain.Workflow> Workflows => Set<Domain.Workflow>();
    public DbSet<FormularioWorkflow> FormulariosWorkflow => Set<FormularioWorkflow>();
    public DbSet<GrupoWf> GruposWf => Set<GrupoWf>();
    public DbSet<MiembroGrupoWf> MiembrosGrupoWf => Set<MiembroGrupoWf>();
    public DbSet<Evidencia> Evidencias => Set<Evidencia>();

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Proceso>(e =>
        {
            e.ToTable("proceso");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id_proceso").UseIdentityAlwaysColumn();
            e.Property(x => x.Nombre).HasColumnName("proceso");
        });
        modelBuilder.Entity<Paso>(e =>
        {
            e.ToTable("paso");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id_paso").UseIdentityAlwaysColumn();
            e.Property(x => x.Numero).HasColumnName("paso");
        });
        modelBuilder.Entity<ConfiguracionPaso>(e =>
        {
            e.ToTable("configuracion_paso");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
        });
        modelBuilder.Entity<PosiblePasoDevolucion>(e =>
        {
            e.ToTable("posibles_pasos_devolucion");
            e.HasKey(x => new { x.Paso, x.Descripcion, x.IdProceso });
        });
        modelBuilder.Entity<Domain.Workflow>(e =>
        {
            e.ToTable("workflow");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id_workflow").UseIdentityAlwaysColumn();
        });
        modelBuilder.Entity<FormularioWorkflow>(e =>
        {
            e.ToTable("formulario_workflow");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
        });
        modelBuilder.Entity<GrupoWf>(e =>
        {
            e.ToTable("grupos_wf");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id_grupo").UseIdentityAlwaysColumn();
        });
        modelBuilder.Entity<MiembroGrupoWf>(e =>
        {
            e.ToTable("miembros_grupo_wf");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
        });
        modelBuilder.Entity<Evidencia>(e =>
        {
            e.ToTable("evidencias");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
        });

        ConvencionesDeNomenclatura.AplicarSnakeCaseAColumnas(modelBuilder);
    }
}

public static class RegistroDeModulo
{
    public static IServiceCollection AgregarModuloWorkflow(this IServiceCollection servicios)
    {
        servicios.AgregarDbContextDeTenant<WorkflowDbContext>();
        servicios.AddScoped<IUnidadDeTrabajoWorkflow>(sp => sp.GetRequiredService<WorkflowDbContext>());

        servicios.AddScoped<IRepositorioDeProcesos, RepositorioDeProcesos>();
        servicios.AddScoped<IRepositorioDePasos, RepositorioDePasos>();
        servicios.AddScoped<IRepositorioDeWorkflows, RepositorioDeWorkflows>();

        servicios.AddScoped<IManejadorDeComando<CrearProceso, Result<int>>, CrearProcesoManejador>();
        servicios.AddScoped<IManejadorDeComando<ActualizarProceso, Result>, ActualizarProcesoManejador>();
        servicios.AddScoped<IManejadorDeComando<EliminarProceso, Result>, EliminarProcesoManejador>();
        servicios.AddScoped<IManejadorDeComando<AgregarPaso, Result<int>>, AgregarPasoManejador>();
        servicios.AddScoped<IManejadorDeComando<IniciarWorkflow, Result<long>>, IniciarWorkflowManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ObtenerProcesoPorId, ProcesoDto?>, ObtenerProcesoPorIdManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ListarProcesos, ResultadoPaginado<ProcesoDto>>, ListarProcesosManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ListarPasosDeProceso, IReadOnlyList<PasoDto>>, ListarPasosDeProcesoManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ObtenerWorkflowPorId, WorkflowDto?>, ObtenerWorkflowPorIdManejador>();

        return servicios;
    }
}
