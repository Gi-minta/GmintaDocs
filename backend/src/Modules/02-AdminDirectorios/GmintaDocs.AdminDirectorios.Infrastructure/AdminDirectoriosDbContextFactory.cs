using GmintaDocs.Multitenancy;
using Microsoft.EntityFrameworkCore;

namespace GmintaDocs.AdminDirectorios.Infrastructure;

/// <summary>Fábrica de diseño para generar migraciones (la BD real se resuelve por empresa en runtime).</summary>
public sealed class AdminDirectoriosDbContextFactory : FabricaDeDisenioDeTenant<AdminDirectoriosDbContext>
{
    protected override AdminDirectoriosDbContext Construir(DbContextOptions<AdminDirectoriosDbContext> opciones)
        => new(opciones);
}
