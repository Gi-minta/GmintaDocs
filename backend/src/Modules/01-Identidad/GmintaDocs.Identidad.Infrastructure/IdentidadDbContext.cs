using GmintaDocs.Identidad.Application;
using GmintaDocs.Identidad.Domain;
using GmintaDocs.Multitenancy;
using Microsoft.EntityFrameworkCore;

namespace GmintaDocs.Identidad.Infrastructure;

/// <summary>
/// Contexto de identidad/seguridad. Vive en la base de datos MAESTRA/control.
/// </summary>
public sealed class IdentidadDbContext : DbContext, IUnidadDeTrabajoIdentidad
{
    public IdentidadDbContext(DbContextOptions<IdentidadDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<EventoIdentidad> EventosIdentidad => Set<EventoIdentidad>();
    public DbSet<SesionEnLinea> SesionesEnLinea => Set<SesionEnLinea>();
    public DbSet<OpcionRol> OpcionesRol => Set<OpcionRol>();

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasMaxLength(128);
            e.Property(x => x.TipoUsuario).HasColumnName("discriminator").HasMaxLength(128);

            e.HasMany(x => x.Roles).WithOne().HasForeignKey(r => r.UserId);
            e.HasMany(x => x.Claims).WithOne().HasForeignKey(c => c.UserId);
            e.HasMany(x => x.Logins).WithOne().HasForeignKey(l => l.UserId);

            e.Metadata.FindNavigation(nameof(Usuario.Roles))!.SetPropertyAccessMode(PropertyAccessMode.Field);
            e.Metadata.FindNavigation(nameof(Usuario.Claims))!.SetPropertyAccessMode(PropertyAccessMode.Field);
            e.Metadata.FindNavigation(nameof(Usuario.Logins))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<UsuarioRol>(e =>
        {
            e.ToTable("user_roles");
            e.HasKey(x => new { x.UserId, x.RoleId });
        });

        modelBuilder.Entity<UsuarioClaim>(e =>
        {
            e.ToTable("user_claims");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<UsuarioLogin>(e =>
        {
            e.ToTable("user_logins");
            e.HasKey(x => new { x.UserId, x.LoginProvider, x.ProviderKey });
        });

        modelBuilder.Entity<Rol>(e =>
        {
            e.ToTable("roles");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasMaxLength(128);
            e.Property(x => x.Nombre).HasColumnName("name");
        });

        modelBuilder.Entity<EventoIdentidad>(e =>
        {
            e.ToTable("eventos_identidad");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
        });

        modelBuilder.Entity<SesionEnLinea>(e =>
        {
            e.ToTable("online");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        });

        modelBuilder.Entity<OpcionRol>(e =>
        {
            e.ToTable("opciones_rol");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
            e.Property(x => x.Rol).HasMaxLength(128);
        });

        ConvencionesDeNomenclatura.AplicarSnakeCaseAColumnas(modelBuilder);
    }
}
