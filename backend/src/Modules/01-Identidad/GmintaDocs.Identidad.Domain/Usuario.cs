using GmintaDocs.SharedKernel;

namespace GmintaDocs.Identidad.Domain;

/// <summary>
/// Usuario del sistema (raíz de agregado). Incluye sus roles, claims y logins externos.
/// La identidad vive en la base de datos maestra/control.
/// </summary>
public sealed class Usuario : AggregateRoot<string>
{
    private readonly List<UsuarioRol> _roles = new();
    private readonly List<UsuarioClaim> _claims = new();
    private readonly List<UsuarioLogin> _logins = new();

    public string? UserName { get; private set; }
    public string? PasswordHash { get; private set; }
    public string? SecurityStamp { get; private set; }
    public string TipoUsuario { get; private set; } = "Usuario"; // columna "discriminator"
    public string? Code { get; private set; }
    public string? FullName { get; private set; }
    public string? Email { get; private set; }
    public DateTime? BirthDay { get; private set; }
    public string? Avatar { get; private set; }
    public bool? Activo { get; private set; }

    public IReadOnlyCollection<UsuarioRol> Roles => _roles.AsReadOnly();
    public IReadOnlyCollection<UsuarioClaim> Claims => _claims.AsReadOnly();
    public IReadOnlyCollection<UsuarioLogin> Logins => _logins.AsReadOnly();

    private Usuario() { }

    private Usuario(string id, string userName, string? fullName, string? email)
    {
        Id = id;
        UserName = userName;
        FullName = fullName;
        Email = email;
        Activo = true;
        SecurityStamp = Guid.NewGuid().ToString("N");
    }

    public static Result<Usuario> Crear(string userName, string? fullName, string? email, string? id = null)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return Result<Usuario>.Fallido("El nombre de usuario es obligatorio.");

        var identificador = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString() : id;
        return Result<Usuario>.Exitoso(new Usuario(identificador, userName.Trim(), fullName, email));
    }

    public void EstablecerPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
        SecurityStamp = Guid.NewGuid().ToString("N");
    }

    public void Activar() => Activo = true;
    public void Desactivar() => Activo = false;

    public Result AsignarRol(string idRol)
    {
        if (string.IsNullOrWhiteSpace(idRol))
            return Result.Fallido("El rol es obligatorio.");
        if (_roles.Any(r => r.RoleId == idRol))
            return Result.Fallido("El usuario ya tiene asignado ese rol.");

        _roles.Add(new UsuarioRol(Id, idRol));
        return Result.Exitoso();
    }

    public void QuitarRol(string idRol) => _roles.RemoveAll(r => r.RoleId == idRol);
}

/// <summary>Relación usuario-rol (tabla user_roles, clave compuesta).</summary>
public sealed class UsuarioRol
{
    public string UserId { get; private set; } = string.Empty;
    public string RoleId { get; private set; } = string.Empty;

    private UsuarioRol() { }
    public UsuarioRol(string userId, string roleId) { UserId = userId; RoleId = roleId; }
}

/// <summary>Claim de un usuario (tabla user_claims).</summary>
public sealed class UsuarioClaim
{
    public int Id { get; private set; }
    public string? ClaimType { get; private set; }
    public string? ClaimValue { get; private set; }
    public string UserId { get; private set; } = string.Empty;

    private UsuarioClaim() { }
    public UsuarioClaim(string userId, string? claimType, string? claimValue)
    {
        UserId = userId; ClaimType = claimType; ClaimValue = claimValue;
    }
}

/// <summary>Login externo de un usuario (tabla user_logins, clave compuesta).</summary>
public sealed class UsuarioLogin
{
    public string UserId { get; private set; } = string.Empty;
    public string LoginProvider { get; private set; } = string.Empty;
    public string ProviderKey { get; private set; } = string.Empty;

    private UsuarioLogin() { }
    public UsuarioLogin(string userId, string loginProvider, string providerKey)
    {
        UserId = userId; LoginProvider = loginProvider; ProviderKey = providerKey;
    }
}
