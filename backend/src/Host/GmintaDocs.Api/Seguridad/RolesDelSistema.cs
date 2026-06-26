namespace GmintaDocs.Api.Seguridad;

/// <summary>
/// Roles bien conocidos del sistema. Se siembran con <c>Id == Nombre</c> para que el valor
/// que viaja en el claim de rol del token (que es el <c>RoleId</c>) coincida con el usado en
/// los atributos <c>[Authorize(Roles = ...)]</c>.
/// </summary>
public static class RolesDelSistema
{
    public const string Administrador = "Administrador";
    public const string Gestor = "Gestor";
    public const string Usuario = "Usuario";

    /// <summary>Roles que el sembrador garantiza en la base maestra al arrancar.</summary>
    public static readonly string[] Todos = [Administrador, Gestor, Usuario];
}
