using GmintaDocs.Colaboracion.Application;
using GmintaDocs.Colaboracion.Domain;
using GmintaDocs.CQRS;
using GmintaDocs.Multitenancy;
using GmintaDocs.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GmintaDocs.Colaboracion.Infrastructure;

/// <summary>Contexto del módulo de colaboración (base de datos por empresa).</summary>
public sealed class ColaboracionDbContext : DbContext, IUnidadDeTrabajoColaboracion
{
    public ColaboracionDbContext(DbContextOptions<ColaboracionDbContext> options) : base(options) { }

    public DbSet<Noticia> Noticias => Set<Noticia>();
    public DbSet<Comentario> Comentarios => Set<Comentario>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Noticia>(e =>
        {
            e.ToTable("noticias");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id_noticia").UseIdentityAlwaysColumn();
        });
        modelBuilder.Entity<Comentario>(e =>
        {
            e.ToTable("comentarios");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
        });
        modelBuilder.Entity<Notificacion>(e =>
        {
            e.ToTable("notificaciones");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
        });

        ConvencionesDeNomenclatura.AplicarSnakeCaseAColumnas(modelBuilder);
    }
}

public static class RegistroDeModulo
{
    public static IServiceCollection AgregarModuloColaboracion(this IServiceCollection servicios)
    {
        servicios.AgregarDbContextDeTenant<ColaboracionDbContext>();
        servicios.AddScoped<IUnidadDeTrabajoColaboracion>(sp => sp.GetRequiredService<ColaboracionDbContext>());

        servicios.AddScoped<IRepositorioDeNoticias, RepositorioDeNoticias>();
        servicios.AddScoped<IRepositorioDeComentarios, RepositorioDeComentarios>();

        servicios.AddScoped<IManejadorDeComando<PublicarNoticia, Result<long>>, PublicarNoticiaManejador>();
        servicios.AddScoped<IManejadorDeComando<ActualizarNoticia, Result>, ActualizarNoticiaManejador>();
        servicios.AddScoped<IManejadorDeComando<EliminarNoticia, Result>, EliminarNoticiaManejador>();
        servicios.AddScoped<IManejadorDeComando<ComentarNoticia, Result<long>>, ComentarNoticiaManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ObtenerNoticiaPorId, NoticiaDto?>, ObtenerNoticiaPorIdManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ListarNoticias, ResultadoPaginado<NoticiaDto>>, ListarNoticiasManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ListarComentariosDeNoticia, IReadOnlyList<ComentarioDto>>, ListarComentariosDeNoticiaManejador>();

        return servicios;
    }
}
