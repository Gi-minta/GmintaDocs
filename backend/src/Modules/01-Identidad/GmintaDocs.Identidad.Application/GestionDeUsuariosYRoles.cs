using GmintaDocs.CQRS;
using GmintaDocs.Identidad.Domain;
using GmintaDocs.SharedKernel;

namespace GmintaDocs.Identidad.Application;

// ---- Comandos ----
public sealed record CrearUsuario(string UserName, string? FullName, string? Email, string? Contrasena = null)
    : IComando<Result<string>>;
public sealed record AsignarRolAUsuario(string IdUsuario, string IdRol) : IComando<Result>;
public sealed record DesactivarUsuario(string IdUsuario) : IComando<Result>;
public sealed record CambiarContrasena(string IdUsuario, string ContrasenaActual, string ContrasenaNueva) : IComando<Result>;
public sealed record CrearRol(string Nombre) : IComando<Result<string>>;

// ---- Consultas ----
public sealed record ObtenerUsuarioPorId(string Id) : IConsulta<UsuarioDto?>;
public sealed record ListarUsuarios(ParametrosDePaginacion Parametros) : IConsulta<ResultadoPaginado<UsuarioDto>>;
public sealed record ListarRoles(ParametrosDePaginacion Parametros) : IConsulta<ResultadoPaginado<RolDto>>;
public sealed record AutenticarUsuario(string UserName, string Contrasena)
    : IConsulta<Result<UsuarioAutenticadoDto>>;

// ---- Manejadores de comando ----
public sealed class CrearUsuarioManejador : IManejadorDeComando<CrearUsuario, Result<string>>
{
    private readonly IRepositorioDeUsuarios _usuarios;
    private readonly IHasheadorDeContrasena _hasheador;
    private readonly IUnidadDeTrabajoIdentidad _uow;

    public CrearUsuarioManejador(IRepositorioDeUsuarios usuarios, IHasheadorDeContrasena hasheador, IUnidadDeTrabajoIdentidad uow)
    {
        _usuarios = usuarios; _hasheador = hasheador; _uow = uow;
    }

    public async Task<Result<string>> ManejarAsync(CrearUsuario comando, CancellationToken cancellationToken)
    {
        var existente = await _usuarios.ObtenerPorUserNameAsync(comando.UserName, cancellationToken);
        if (existente is not null)
            return Result<string>.Fallido($"Ya existe un usuario '{comando.UserName}'.");

        var creacion = Usuario.Crear(comando.UserName, comando.FullName, comando.Email);
        if (!creacion.EsExitoso)
            return Result<string>.Fallido(creacion.Error!);

        if (!string.IsNullOrWhiteSpace(comando.Contrasena))
            creacion.Valor.EstablecerPasswordHash(_hasheador.Hashear(comando.Contrasena));

        await _usuarios.AgregarAsync(creacion.Valor, cancellationToken);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result<string>.Exitoso(creacion.Valor.Id);
    }
}

public sealed class AsignarRolAUsuarioManejador : IManejadorDeComando<AsignarRolAUsuario, Result>
{
    private readonly IRepositorioDeUsuarios _usuarios;
    private readonly IRepositorioDeRoles _roles;
    private readonly IUnidadDeTrabajoIdentidad _uow;

    public AsignarRolAUsuarioManejador(IRepositorioDeUsuarios usuarios, IRepositorioDeRoles roles, IUnidadDeTrabajoIdentidad uow)
    {
        _usuarios = usuarios; _roles = roles; _uow = uow;
    }

    public async Task<Result> ManejarAsync(AsignarRolAUsuario comando, CancellationToken cancellationToken)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(comando.IdUsuario, cancellationToken);
        if (usuario is null)
            return Result.Fallido($"No existe el usuario {comando.IdUsuario}.");

        var rol = await _roles.ObtenerPorIdAsync(comando.IdRol, cancellationToken);
        if (rol is null)
            return Result.Fallido($"No existe el rol {comando.IdRol}.");

        var resultado = usuario.AsignarRol(comando.IdRol);
        if (!resultado.EsExitoso)
            return resultado;

        _usuarios.Actualizar(usuario);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result.Exitoso();
    }
}

public sealed class DesactivarUsuarioManejador : IManejadorDeComando<DesactivarUsuario, Result>
{
    private readonly IRepositorioDeUsuarios _usuarios;
    private readonly IUnidadDeTrabajoIdentidad _uow;

    public DesactivarUsuarioManejador(IRepositorioDeUsuarios usuarios, IUnidadDeTrabajoIdentidad uow)
    {
        _usuarios = usuarios; _uow = uow;
    }

    public async Task<Result> ManejarAsync(DesactivarUsuario comando, CancellationToken cancellationToken)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(comando.IdUsuario, cancellationToken);
        if (usuario is null)
            return Result.Fallido($"No existe el usuario {comando.IdUsuario}.");

        usuario.Desactivar();
        _usuarios.Actualizar(usuario);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result.Exitoso();
    }
}

public sealed class CambiarContrasenaManejador : IManejadorDeComando<CambiarContrasena, Result>
{
    private readonly IRepositorioDeUsuarios _usuarios;
    private readonly IHasheadorDeContrasena _hasheador;
    private readonly IUnidadDeTrabajoIdentidad _uow;

    public CambiarContrasenaManejador(IRepositorioDeUsuarios usuarios, IHasheadorDeContrasena hasheador, IUnidadDeTrabajoIdentidad uow)
    {
        _usuarios = usuarios; _hasheador = hasheador; _uow = uow;
    }

    public async Task<Result> ManejarAsync(CambiarContrasena comando, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(comando.ContrasenaNueva) || comando.ContrasenaNueva.Length < 8)
            return Result.Fallido("La nueva contraseña debe tener al menos 8 caracteres.");

        var usuario = await _usuarios.ObtenerPorIdAsync(comando.IdUsuario, cancellationToken);
        if (usuario is null)
            return Result.Fallido($"No existe el usuario {comando.IdUsuario}.");

        if (string.IsNullOrEmpty(usuario.PasswordHash) ||
            !_hasheador.Verificar(comando.ContrasenaActual, usuario.PasswordHash))
            return Result.Fallido("La contraseña actual no es correcta.");

        usuario.EstablecerPasswordHash(_hasheador.Hashear(comando.ContrasenaNueva));
        _usuarios.Actualizar(usuario);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result.Exitoso();
    }
}

public sealed class CrearRolManejador : IManejadorDeComando<CrearRol, Result<string>>
{
    private readonly IRepositorioDeRoles _roles;
    private readonly IUnidadDeTrabajoIdentidad _uow;

    public CrearRolManejador(IRepositorioDeRoles roles, IUnidadDeTrabajoIdentidad uow) { _roles = roles; _uow = uow; }

    public async Task<Result<string>> ManejarAsync(CrearRol comando, CancellationToken cancellationToken)
    {
        var creacion = Rol.Crear(comando.Nombre);
        if (!creacion.EsExitoso)
            return Result<string>.Fallido(creacion.Error!);

        await _roles.AgregarAsync(creacion.Valor, cancellationToken);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result<string>.Exitoso(creacion.Valor.Id);
    }
}

// ---- Manejadores de consulta ----
public sealed class ObtenerUsuarioPorIdManejador : IManejadorDeConsulta<ObtenerUsuarioPorId, UsuarioDto?>
{
    private readonly IRepositorioDeUsuarios _usuarios;
    public ObtenerUsuarioPorIdManejador(IRepositorioDeUsuarios usuarios) => _usuarios = usuarios;

    public async Task<UsuarioDto?> ManejarAsync(ObtenerUsuarioPorId consulta, CancellationToken cancellationToken)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(consulta.Id, cancellationToken);
        return usuario?.ADto();
    }
}

public sealed class ListarUsuariosManejador : IManejadorDeConsulta<ListarUsuarios, ResultadoPaginado<UsuarioDto>>
{
    private readonly IRepositorioDeUsuarios _usuarios;
    public ListarUsuariosManejador(IRepositorioDeUsuarios usuarios) => _usuarios = usuarios;

    public async Task<ResultadoPaginado<UsuarioDto>> ManejarAsync(ListarUsuarios consulta, CancellationToken cancellationToken)
    {
        var pagina = await _usuarios.ListarPaginadoAsync(consulta.Parametros, cancellationToken);
        return pagina.Mapear(u => u.ADto());
    }
}

public sealed class ListarRolesManejador : IManejadorDeConsulta<ListarRoles, ResultadoPaginado<RolDto>>
{
    private readonly IRepositorioDeRoles _roles;
    public ListarRolesManejador(IRepositorioDeRoles roles) => _roles = roles;

    public async Task<ResultadoPaginado<RolDto>> ManejarAsync(ListarRoles consulta, CancellationToken cancellationToken)
    {
        var pagina = await _roles.ListarPaginadoAsync(consulta.Parametros, cancellationToken);
        return pagina.Mapear(r => r.ADto());
    }
}

public sealed class AutenticarUsuarioManejador
    : IManejadorDeConsulta<AutenticarUsuario, Result<UsuarioAutenticadoDto>>
{
    private readonly IRepositorioDeUsuarios _usuarios;
    private readonly IHasheadorDeContrasena _hasheador;

    public AutenticarUsuarioManejador(IRepositorioDeUsuarios usuarios, IHasheadorDeContrasena hasheador)
    {
        _usuarios = usuarios; _hasheador = hasheador;
    }

    public async Task<Result<UsuarioAutenticadoDto>> ManejarAsync(AutenticarUsuario consulta, CancellationToken cancellationToken)
    {
        // Mensaje genérico a propósito: no se revela si falla el usuario o la contraseña.
        var usuario = await _usuarios.ObtenerPorUserNameAsync(consulta.UserName, cancellationToken);
        if (usuario is null || usuario.Activo != true)
            return Result<UsuarioAutenticadoDto>.Fallido("Credenciales inválidas.");

        if (string.IsNullOrEmpty(usuario.PasswordHash) ||
            !_hasheador.Verificar(consulta.Contrasena, usuario.PasswordHash))
            return Result<UsuarioAutenticadoDto>.Fallido("Credenciales inválidas.");

        var dto = new UsuarioAutenticadoDto(
            usuario.Id, usuario.UserName, usuario.FullName,
            usuario.Roles.Select(r => r.RoleId).ToList());
        return Result<UsuarioAutenticadoDto>.Exitoso(dto);
    }
}
