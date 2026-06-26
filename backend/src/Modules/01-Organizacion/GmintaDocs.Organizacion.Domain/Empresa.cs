using GmintaDocs.SharedKernel;

namespace GmintaDocs.Organizacion.Domain;

/// <summary>
/// Empresa (tenant). Raíz del núcleo multi-tenant: toda entidad de negocio depende de su contexto.
/// Vive en la base de datos maestra/control; cada empresa tiene además su propia base de datos dedicada.
/// </summary>
public sealed class Empresa : AggregateRoot<long>
{
    public string RazonSocial { get; private set; } = string.Empty;
    public string Nit { get; private set; } = string.Empty;
    public string? Direccion { get; private set; }
    public string? Ciudad { get; private set; }
    public string? Url { get; private set; }
    public string? Email { get; private set; }
    public string? Telefono { get; private set; }
    public string? Notas { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;

    private Empresa() { }

    private Empresa(string razonSocial, string nit, string usuario, string host)
    {
        RazonSocial = razonSocial;
        Nit = nit;
        Usuario = usuario;
        Host = host;
        Fecha = DateTime.UtcNow;
    }

    public static Result<Empresa> Crear(string razonSocial, string nit, string usuario, string host)
    {
        if (string.IsNullOrWhiteSpace(razonSocial))
            return Result<Empresa>.Fallido("La razón social es obligatoria.");
        if (string.IsNullOrWhiteSpace(nit))
            return Result<Empresa>.Fallido("El NIT es obligatorio.");

        return Result<Empresa>.Exitoso(new Empresa(razonSocial.Trim(), nit.Trim(), usuario, host));
    }

    public Result ActualizarDatos(string razonSocial, string? direccion, string? ciudad,
        string? url, string? email, string? telefono, string? notas)
    {
        if (string.IsNullOrWhiteSpace(razonSocial))
            return Result.Fallido("La razón social es obligatoria.");

        RazonSocial = razonSocial.Trim();
        Direccion = direccion;
        Ciudad = ciudad;
        Url = url;
        Email = email;
        Telefono = telefono;
        Notas = notas;
        return Result.Exitoso();
    }
}
