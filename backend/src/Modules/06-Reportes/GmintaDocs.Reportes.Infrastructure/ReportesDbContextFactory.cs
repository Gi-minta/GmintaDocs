using GmintaDocs.Multitenancy;
using Microsoft.EntityFrameworkCore;

namespace GmintaDocs.Reportes.Infrastructure;

/// <summary>Fábrica de diseño para generar migraciones (la BD real se resuelve por empresa en runtime).</summary>
public sealed class ReportesDbContextFactory : FabricaDeDisenioDeTenant<ReportesDbContext>
{
    protected override ReportesDbContext Construir(DbContextOptions<ReportesDbContext> opciones)
        => new(opciones);
}
