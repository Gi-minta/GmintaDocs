using GmintaDocs.Multitenancy;
using Microsoft.EntityFrameworkCore;

namespace GmintaDocs.GestionDocumental.Infrastructure;

/// <summary>Fábrica de diseño para generar migraciones (la BD real se resuelve por empresa en runtime).</summary>
public sealed class GestionDocumentalDbContextFactory : FabricaDeDisenioDeTenant<GestionDocumentalDbContext>
{
    protected override GestionDocumentalDbContext Construir(DbContextOptions<GestionDocumentalDbContext> opciones)
        => new(opciones);
}
