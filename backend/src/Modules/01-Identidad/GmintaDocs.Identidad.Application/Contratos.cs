using GmintaDocs.Identidad.Domain;
using GmintaDocs.SharedKernel;

namespace GmintaDocs.Identidad.Application;

/// <summary>Unidad de trabajo específica del módulo (evita colisión de DI entre los DbContext de cada módulo).</summary>
public interface IUnidadDeTrabajoIdentidad : IUnidadDeTrabajo { }

public interface IRepositorioDeUsuarios : IRepositorio<Usuario, string>
{
    Task<Usuario?> ObtenerPorUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<ResultadoPaginado<Usuario>> ListarPaginadoAsync(ParametrosDePaginacion parametros, CancellationToken cancellationToken = default);
}

public interface IRepositorioDeRoles : IRepositorio<Rol, string>
{
    Task<ResultadoPaginado<Rol>> ListarPaginadoAsync(ParametrosDePaginacion parametros, CancellationToken cancellationToken = default);
}

public sealed record UsuarioDto(
    string Id, string? UserName, string? FullName, string? Email, bool? Activo, IReadOnlyList<string> Roles);

public sealed record RolDto(string Id, string Nombre);

/// <summary>Datos del usuario tras validar sus credenciales (insumo para emitir el token).</summary>
public sealed record UsuarioAutenticadoDto(
    string Id, string? UserName, string? FullName, IReadOnlyList<string> Roles);

/// <summary>
/// Hashea y verifica contraseñas. La implementación concreta (PBKDF2) vive en Infrastructure
/// para mantener la capa de aplicación libre de detalles criptográficos.
/// </summary>
public interface IHasheadorDeContrasena
{
    string Hashear(string contrasena);
    bool Verificar(string contrasena, string hashAlmacenado);
}

public static class MapeosIdentidad
{
    public static UsuarioDto ADto(this Usuario u) =>
        new(u.Id, u.UserName, u.FullName, u.Email, u.Activo, u.Roles.Select(r => r.RoleId).ToList());

    public static RolDto ADto(this Rol r) => new(r.Id, r.Nombre);
}
