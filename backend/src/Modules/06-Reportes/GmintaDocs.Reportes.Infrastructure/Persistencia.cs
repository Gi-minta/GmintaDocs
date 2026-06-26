using GmintaDocs.CQRS;
using GmintaDocs.Multitenancy;
using GmintaDocs.Reportes.Application;
using GmintaDocs.Reportes.Domain;
using GmintaDocs.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GmintaDocs.Reportes.Infrastructure;

/// <summary>Contexto del módulo de reportes (base de datos por empresa).</summary>
public sealed class ReportesDbContext : DbContext, IUnidadDeTrabajoReportes
{
    public ReportesDbContext(DbContextOptions<ReportesDbContext> options) : base(options) { }

    public DbSet<CategoriaReporte> CategoriasReporte => Set<CategoriaReporte>();
    public DbSet<ReporteSsrs> ReportesSsrs => Set<ReporteSsrs>();

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CategoriaReporte>(e =>
        {
            e.ToTable("categoria_reportes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id_categoria").UseIdentityAlwaysColumn();
        });
        modelBuilder.Entity<ReporteSsrs>(e =>
        {
            e.ToTable("reportes_ssrs");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id_reporte").UseIdentityAlwaysColumn();
        });

        ConvencionesDeNomenclatura.AplicarSnakeCaseAColumnas(modelBuilder);
    }
}

public static class RegistroDeModulo
{
    public static IServiceCollection AgregarModuloReportes(this IServiceCollection servicios)
    {
        servicios.AgregarDbContextDeTenant<ReportesDbContext>();
        servicios.AddScoped<IUnidadDeTrabajoReportes>(sp => sp.GetRequiredService<ReportesDbContext>());

        servicios.AddScoped<IRepositorioDeCategoriasReporte, RepositorioDeCategoriasReporte>();
        servicios.AddScoped<IRepositorioDeReportes, RepositorioDeReportes>();

        servicios.AddScoped<IManejadorDeComando<CrearCategoriaReporte, Result<long>>, CrearCategoriaReporteManejador>();
        servicios.AddScoped<IManejadorDeComando<CrearReporte, Result<long>>, CrearReporteManejador>();
        servicios.AddScoped<IManejadorDeComando<ActualizarReporte, Result>, ActualizarReporteManejador>();
        servicios.AddScoped<IManejadorDeComando<EliminarReporte, Result>, EliminarReporteManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ListarCategoriasReporte, ResultadoPaginado<CategoriaReporteDto>>, ListarCategoriasReporteManejador>();
        servicios.AddScoped<IManejadorDeConsulta<ListarReportesPorCategoria, IReadOnlyList<ReporteDto>>, ListarReportesPorCategoriaManejador>();

        return servicios;
    }
}
