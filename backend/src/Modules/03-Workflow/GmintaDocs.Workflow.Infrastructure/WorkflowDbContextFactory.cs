using GmintaDocs.Multitenancy;
using Microsoft.EntityFrameworkCore;

namespace GmintaDocs.Workflow.Infrastructure;

/// <summary>Fábrica de diseño para generar migraciones (la BD real se resuelve por empresa en runtime).</summary>
public sealed class WorkflowDbContextFactory : FabricaDeDisenioDeTenant<WorkflowDbContext>
{
    protected override WorkflowDbContext Construir(DbContextOptions<WorkflowDbContext> opciones)
        => new(opciones);
}
