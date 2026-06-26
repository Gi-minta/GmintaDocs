using Microsoft.AspNetCore.Authorization;

namespace GmintaDocs.Api.Seguridad;

/// <summary>
/// Políticas de autorización del sistema. Centraliza los nombres y el registro para que
/// los controllers no repitan listas de roles sueltas.
/// </summary>
public static class PoliticasDeAutorizacion
{
    /// <summary>Acceso a los módulos de negocio: requiere rol Administrador o Gestor.</summary>
    public const string Gestion = "Gestion";

    public static void Registrar(AuthorizationOptions opciones)
    {
        opciones.AddPolicy(Gestion, p =>
            p.RequireRole(RolesDelSistema.Administrador, RolesDelSistema.Gestor));
    }
}
