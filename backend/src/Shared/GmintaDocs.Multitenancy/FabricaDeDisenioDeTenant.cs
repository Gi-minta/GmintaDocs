using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GmintaDocs.Multitenancy;

/// <summary>
/// Base para las fábricas de diseño (<see cref="IDesignTimeDbContextFactory{TContext}"/>) de los
/// DbContext por empresa. A tiempo de diseño (p. ej. <c>dotnet ef migrations add</c>) no hay una
/// empresa activa, por lo que la cadena de conexión se toma de la variable de entorno
/// <c>GMINTA_TENANT_CNX</c> o, en su defecto, de una base de datos local de diseño. La conexión
/// no se abre para generar migraciones: sólo se necesita para construir el modelo.
/// </summary>
public abstract class FabricaDeDisenioDeTenant<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : DbContext
{
    public const string VariableDeEntorno = "GMINTA_TENANT_CNX";

    private const string CadenaDeDisenioPorDefecto =
        "Host=localhost;Port=5432;Database=gmintadocs_empresa_design;Username=postgres;Password=postgres";

    public TContext CreateDbContext(string[] args)
    {
        var cadena = Environment.GetEnvironmentVariable(VariableDeEntorno) ?? CadenaDeDisenioPorDefecto;
        var opciones = new DbContextOptionsBuilder<TContext>().UseNpgsql(cadena).Options;
        return Construir(opciones);
    }

    protected abstract TContext Construir(DbContextOptions<TContext> opciones);
}
