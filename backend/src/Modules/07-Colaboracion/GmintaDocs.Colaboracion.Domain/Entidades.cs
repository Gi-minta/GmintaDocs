using GmintaDocs.SharedKernel;

namespace GmintaDocs.Colaboracion.Domain;

/// <summary>Noticia publicada en el portal (tabla noticias).</summary>
public sealed class Noticia : AggregateRoot<long>
{
    public string Titulo { get; private set; } = string.Empty;
    public string Autor { get; private set; } = string.Empty;
    public string Avatar { get; private set; } = string.Empty;
    public string Texto { get; private set; } = string.Empty;
    public string Usuario { get; private set; } = string.Empty;
    public DateTime FechaPublicacion { get; private set; }
    public string Host { get; private set; } = string.Empty;

    private Noticia() { }

    public static Result<Noticia> Crear(string titulo, string autor, string avatar, string texto,
        string usuario, string host)
    {
        if (string.IsNullOrWhiteSpace(titulo)) return Result<Noticia>.Fallido("El título es obligatorio.");
        if (string.IsNullOrWhiteSpace(texto)) return Result<Noticia>.Fallido("El texto es obligatorio.");

        return Result<Noticia>.Exitoso(new Noticia
        {
            Titulo = titulo.Trim(), Autor = autor, Avatar = avatar, Texto = texto,
            Usuario = usuario, Host = host, FechaPublicacion = DateTime.UtcNow
        });
    }

    public Result Editar(string titulo, string texto)
    {
        if (string.IsNullOrWhiteSpace(titulo)) return Result.Fallido("El título es obligatorio.");
        if (string.IsNullOrWhiteSpace(texto)) return Result.Fallido("El texto es obligatorio.");
        Titulo = titulo.Trim();
        Texto = texto;
        return Result.Exitoso();
    }
}

/// <summary>Comentario a una noticia (tabla comentarios).</summary>
public sealed class Comentario : AggregateRoot<long>
{
    public string Autor { get; private set; } = string.Empty;
    public string Avatar { get; private set; } = string.Empty;
    public string Texto { get; private set; } = string.Empty;
    public string Usuario { get; private set; } = string.Empty;
    public DateTime FechaPublicacion { get; private set; }
    public string Host { get; private set; } = string.Empty;
    public long IdNoticia { get; private set; }

    private Comentario() { }

    public static Result<Comentario> Crear(long idNoticia, string autor, string? avatar, string texto,
        string usuario, string host)
    {
        if (idNoticia <= 0) return Result<Comentario>.Fallido("El comentario debe pertenecer a una noticia válida.");
        if (string.IsNullOrWhiteSpace(texto)) return Result<Comentario>.Fallido("El texto del comentario es obligatorio.");

        return Result<Comentario>.Exitoso(new Comentario
        {
            IdNoticia = idNoticia, Autor = autor ?? string.Empty, Avatar = avatar ?? string.Empty,
            Texto = texto, Usuario = usuario, Host = host, FechaPublicacion = DateTime.UtcNow
        });
    }
}

/// <summary>Correo/notificación saliente (tabla notificaciones).</summary>
public sealed class Notificacion : AggregateRoot<long>
{
    public string Remitente { get; private set; } = string.Empty;
    public string Para { get; private set; } = string.Empty;
    public string? Copia { get; private set; }
    public string? CopiaOculta { get; private set; }
    public string Asunto { get; private set; } = string.Empty;
    public bool EsHtml { get; private set; }
    public string Cuerpo { get; private set; } = string.Empty;
    public string? Adjuntos { get; private set; }
    public bool Enviado { get; private set; }
    public DateTime FechaEnvio { get; private set; }
    public DateTime Fecha { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public string Host { get; private set; } = string.Empty;

    private Notificacion() { }

    public void MarcarEnviada()
    {
        Enviado = true;
        FechaEnvio = DateTime.UtcNow;
    }
}
