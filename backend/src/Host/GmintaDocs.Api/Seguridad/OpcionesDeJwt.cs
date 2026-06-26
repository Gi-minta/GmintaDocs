namespace GmintaDocs.Api.Seguridad;

/// <summary>Opciones de firma/validación del token JWT (sección "Jwt" de la configuración).</summary>
public sealed class OpcionesDeJwt
{
    public const string Seccion = "Jwt";

    public string Emisor { get; init; } = "GmintaDocs";
    public string Audiencia { get; init; } = "GmintaDocs";
    public string ClaveSecreta { get; init; } = string.Empty;
    public int MinutosDeVigencia { get; init; } = 480;
}
