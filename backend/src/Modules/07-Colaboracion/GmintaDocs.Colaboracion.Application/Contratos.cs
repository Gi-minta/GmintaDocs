using GmintaDocs.Colaboracion.Domain;
using GmintaDocs.SharedKernel;

namespace GmintaDocs.Colaboracion.Application;

/// <summary>Unidad de trabajo del módulo (evita colisión de DI entre los DbContext por módulo).</summary>
public interface IUnidadDeTrabajoColaboracion : IUnidadDeTrabajo { }

public interface IRepositorioDeNoticias : IRepositorio<Noticia, long>
{
    Task<ResultadoPaginado<Noticia>> ListarPaginadoAsync(ParametrosDePaginacion parametros, CancellationToken cancellationToken = default);
}

public interface IRepositorioDeComentarios : IRepositorio<Comentario, long>
{
    Task<IReadOnlyList<Comentario>> ListarPorNoticiaAsync(long idNoticia, CancellationToken cancellationToken = default);
}

public sealed record NoticiaDto(long Id, string Titulo, string Autor, string Texto, DateTime FechaPublicacion);
public sealed record ComentarioDto(long Id, long IdNoticia, string Autor, string Texto, DateTime FechaPublicacion);

/// <summary>Mapeos explícitos dominio → DTO (sin AutoMapper, por decisión de stack).</summary>
public static class MapeosColaboracion
{
    public static NoticiaDto ADto(this Noticia n) => new(n.Id, n.Titulo, n.Autor, n.Texto, n.FechaPublicacion);

    public static ComentarioDto ADto(this Comentario c) => new(c.Id, c.IdNoticia, c.Autor, c.Texto, c.FechaPublicacion);
}
