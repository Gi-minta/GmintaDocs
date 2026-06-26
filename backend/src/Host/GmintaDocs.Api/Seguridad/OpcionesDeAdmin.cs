namespace GmintaDocs.Api.Seguridad;

/// <summary>
/// Credenciales del administrador inicial que el sembrador crea en la base maestra al arrancar.
/// Configurable bajo la sección <c>Seguridad:Admin</c>.
/// </summary>
public sealed class OpcionesDeAdmin
{
    public const string Seccion = "Seguridad:Admin";

    public string UserName { get; set; } = "admin";
    public string? FullName { get; set; } = "Administrador";
    public string? Email { get; set; }
    public string Contrasena { get; set; } = string.Empty;
}
