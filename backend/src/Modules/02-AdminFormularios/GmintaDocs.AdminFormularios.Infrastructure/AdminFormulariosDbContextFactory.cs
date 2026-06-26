using GmintaDocs.Multitenancy;
using Microsoft.EntityFrameworkCore;

namespace GmintaDocs.AdminFormularios.Infrastructure;

/// <summary>Fábrica de diseño para generar migraciones (la BD real se resuelve por empresa en runtime).</summary>
public sealed class AdminFormulariosDbContextFactory : FabricaDeDisenioDeTenant<AdminFormulariosDbContext>
{
    protected override AdminFormulariosDbContext Construir(DbContextOptions<AdminFormulariosDbContext> opciones)
        => new(opciones);
}
