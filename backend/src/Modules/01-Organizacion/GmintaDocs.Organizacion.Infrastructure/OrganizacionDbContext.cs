using GmintaDocs.Multitenancy;
using GmintaDocs.Organizacion.Application;
using GmintaDocs.Organizacion.Domain;
using Microsoft.EntityFrameworkCore;

namespace GmintaDocs.Organizacion.Infrastructure;

/// <summary>
/// Contexto de persistencia del núcleo organizacional. Vive en la base de datos
/// MAESTRA/control (no por empresa), porque registra los propios tenants.
/// </summary>
public sealed class OrganizacionDbContext : DbContext, IUnidadDeTrabajoOrganizacion
{
    public OrganizacionDbContext(DbContextOptions<OrganizacionDbContext> options) : base(options) { }

    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Sucursal> Sucursales => Set<Sucursal>();
    public DbSet<RolEmpresa> RolesEmpresa => Set<RolEmpresa>();
    public DbSet<RadicadoEmpresa> RadicadosEmpresa => Set<RadicadoEmpresa>();
    public DbSet<RadicadoSucursal> RadicadosSucursal => Set<RadicadoSucursal>();
    public DbSet<Parametro> Parametros => Set<Parametro>();
    public DbSet<Feriado> Feriados => Set<Feriado>();

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Empresa>(e =>
        {
            e.ToTable("empresas");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id_empresa").UseIdentityAlwaysColumn();
        });

        modelBuilder.Entity<Sucursal>(e =>
        {
            e.ToTable("sucursales");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id_sucursal").UseIdentityAlwaysColumn();
        });

        modelBuilder.Entity<RolEmpresa>(e =>
        {
            e.ToTable("roles_empresa");
            e.HasKey(x => new { x.IdEmpresa, x.IdRol });
        });

        modelBuilder.Entity<RadicadoEmpresa>(e =>
        {
            e.ToTable("radicados_empresa");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
        });

        modelBuilder.Entity<RadicadoSucursal>(e =>
        {
            e.ToTable("radicados_sucursal");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
        });

        modelBuilder.Entity<Parametro>(e =>
        {
            e.ToTable("parametros");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id_parametro").UseIdentityAlwaysColumn();
            e.Property(x => x.IdLogico).HasColumnName("id");
        });

        modelBuilder.Entity<Feriado>(e =>
        {
            e.ToTable("feriados");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
        });

        ConvencionesDeNomenclatura.AplicarSnakeCaseAColumnas(modelBuilder);
    }
}
