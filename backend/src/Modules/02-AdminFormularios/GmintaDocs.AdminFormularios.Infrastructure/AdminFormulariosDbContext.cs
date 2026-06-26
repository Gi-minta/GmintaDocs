using GmintaDocs.AdminFormularios.Application;
using GmintaDocs.AdminFormularios.Domain;
using GmintaDocs.CQRS;
using GmintaDocs.Multitenancy;
using GmintaDocs.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GmintaDocs.AdminFormularios.Infrastructure;

/// <summary>Contexto del módulo de administración de formularios (base de datos por empresa).</summary>
public sealed class AdminFormulariosDbContext : DbContext, IUnidadDeTrabajoAdminFormularios
{
    public AdminFormulariosDbContext(DbContextOptions<AdminFormulariosDbContext> options) : base(options) { }

    public DbSet<Formulario> Formularios => Set<Formulario>();
    public DbSet<Campo> Campos => Set<Campo>();
    public DbSet<Copia> Copias => Set<Copia>();
    public DbSet<Lista> Listas => Set<Lista>();
    public DbSet<ItemLista> ItemsLista => Set<ItemLista>();
    public DbSet<MensajeNotificacion> MensajesNotificacion => Set<MensajeNotificacion>();
    public DbSet<ParametroMensaje> ParametrosMensaje => Set<ParametroMensaje>();

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Formulario>(e =>
        {
            e.ToTable("formularios");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id_formulario").UseIdentityAlwaysColumn();
        });
        modelBuilder.Entity<Campo>(e =>
        {
            e.ToTable("campos");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id_campo").UseIdentityAlwaysColumn();
        });
        modelBuilder.Entity<Copia>(e =>
        {
            e.ToTable("copias");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
        });
        modelBuilder.Entity<Lista>(e =>
        {
            e.ToTable("lista");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id_lista").UseIdentityAlwaysColumn();
        });
        modelBuilder.Entity<ItemLista>(e =>
        {
            e.ToTable("item_lista");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id_item").UseIdentityAlwaysColumn();
        });
        modelBuilder.Entity<MensajeNotificacion>(e =>
        {
            e.ToTable("mensajes_notificacion");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
        });
        modelBuilder.Entity<ParametroMensaje>(e =>
        {
            e.ToTable("parametros_mensaje");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id_parametro").UseIdentityAlwaysColumn();
        });

        ConvencionesDeNomenclatura.AplicarSnakeCaseAColumnas(modelBuilder);
    }
}

public static class RegistroDeModulo
{
    public static IServiceCollection AgregarModuloAdminFormularios(this IServiceCollection servicios)
    {
        servicios.AgregarDbContextDeTenant<AdminFormulariosDbContext>();
        servicios.AddScoped<IUnidadDeTrabajoAdminFormularios>(sp => sp.GetRequiredService<AdminFormulariosDbContext>());

        servicios.AddScoped<IRepositorioDeFormularios, RepositorioDeFormularios>();
        servicios.AddScoped<IRepositorioDeCampos, RepositorioDeCampos>();

        servicios.AddScoped<IManejadorDeComando<CrearFormulario, Result<long>>, CrearFormularioManejador>();
        servicios.AddScoped<IManejadorDeComando<ActualizarFormulario, Result>, ActualizarFormularioManejador>();
        servicios.AddScoped<IManejadorDeComando<EliminarFormulario, Result>, EliminarFormularioManejador>();
        servicios.AddScoped<IManejadorDeComando<AgregarCampo, Result<long>>, AgregarCampoManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ObtenerFormularioPorId, FormularioDto?>, ObtenerFormularioPorIdManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ListarFormularios, ResultadoPaginado<FormularioDto>>, ListarFormulariosManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ListarCamposDeFormulario, IReadOnlyList<CampoDto>>, ListarCamposDeFormularioManejador>();

        return servicios;
    }
}
