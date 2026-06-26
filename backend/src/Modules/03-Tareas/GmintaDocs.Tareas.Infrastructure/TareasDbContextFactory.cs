using GmintaDocs.Multitenancy;
using Microsoft.EntityFrameworkCore;

namespace GmintaDocs.Tareas.Infrastructure;

/// <summary>Fábrica de diseño para generar migraciones (la BD real se resuelve por empresa en runtime).</summary>
public sealed class TareasDbContextFactory : FabricaDeDisenioDeTenant<TareasDbContext>
{
    protected override TareasDbContext Construir(DbContextOptions<TareasDbContext> opciones)
        => new(opciones);
}
