using GmintaDocs.Multitenancy;
using Microsoft.EntityFrameworkCore;

namespace GmintaDocs.Plantillas.Infrastructure;

/// <summary>Fábrica de diseño para generar migraciones (la BD real se resuelve por empresa en runtime).</summary>
public sealed class PlantillasDbContextFactory : FabricaDeDisenioDeTenant<PlantillasDbContext>
{
    protected override PlantillasDbContext Construir(DbContextOptions<PlantillasDbContext> opciones)
        => new(opciones);
}
