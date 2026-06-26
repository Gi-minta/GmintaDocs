using GmintaDocs.Colaboracion.Domain;
using GmintaDocs.CQRS;
using GmintaDocs.SharedKernel;

namespace GmintaDocs.Colaboracion.Application;

// ---- Comandos ----
public sealed record PublicarNoticia(string Titulo, string Autor, string? Avatar, string Texto,
    string Usuario, string Host) : IComando<Result<long>>;

public sealed record ActualizarNoticia(long Id, string Titulo, string Texto) : IComando<Result>;
public sealed record EliminarNoticia(long Id) : IComando<Result>;

public sealed record ComentarNoticia(long IdNoticia, string Autor, string? Avatar, string Texto,
    string Usuario, string Host) : IComando<Result<long>>;

// ---- Consultas ----
public sealed record ObtenerNoticiaPorId(long Id) : IConsulta<NoticiaDto?>;
public sealed record ListarNoticias(ParametrosDePaginacion Parametros) : IConsulta<ResultadoPaginado<NoticiaDto>>;
public sealed record ListarComentariosDeNoticia(long IdNoticia) : IConsulta<IReadOnlyList<ComentarioDto>>;

// ---- Manejadores ----
public sealed class PublicarNoticiaManejador : IManejadorDeComando<PublicarNoticia, Result<long>>
{
    private readonly IRepositorioDeNoticias _noticias;
    private readonly IUnidadDeTrabajoColaboracion _uow;

    public PublicarNoticiaManejador(IRepositorioDeNoticias noticias, IUnidadDeTrabajoColaboracion uow)
    {
        _noticias = noticias;
        _uow = uow;
    }

    public async Task<Result<long>> ManejarAsync(PublicarNoticia comando, CancellationToken cancellationToken)
    {
        var creacion = Noticia.Crear(comando.Titulo, comando.Autor, comando.Avatar ?? string.Empty,
            comando.Texto, comando.Usuario, comando.Host);
        if (!creacion.EsExitoso)
            return Result<long>.Fallido(creacion.Error!);

        await _noticias.AgregarAsync(creacion.Valor, cancellationToken);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result<long>.Exitoso(creacion.Valor.Id);
    }
}

public sealed class ActualizarNoticiaManejador : IManejadorDeComando<ActualizarNoticia, Result>
{
    private readonly IRepositorioDeNoticias _noticias;
    private readonly IUnidadDeTrabajoColaboracion _uow;

    public ActualizarNoticiaManejador(IRepositorioDeNoticias noticias, IUnidadDeTrabajoColaboracion uow)
    {
        _noticias = noticias; _uow = uow;
    }

    public async Task<Result> ManejarAsync(ActualizarNoticia comando, CancellationToken cancellationToken)
    {
        var noticia = await _noticias.ObtenerPorIdAsync(comando.Id, cancellationToken);
        if (noticia is null)
            return Result.Fallido($"No existe la noticia {comando.Id}.");

        var resultado = noticia.Editar(comando.Titulo, comando.Texto);
        if (!resultado.EsExitoso)
            return resultado;

        _noticias.Actualizar(noticia);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result.Exitoso();
    }
}

public sealed class EliminarNoticiaManejador : IManejadorDeComando<EliminarNoticia, Result>
{
    private readonly IRepositorioDeNoticias _noticias;
    private readonly IUnidadDeTrabajoColaboracion _uow;

    public EliminarNoticiaManejador(IRepositorioDeNoticias noticias, IUnidadDeTrabajoColaboracion uow)
    {
        _noticias = noticias; _uow = uow;
    }

    public async Task<Result> ManejarAsync(EliminarNoticia comando, CancellationToken cancellationToken)
    {
        var noticia = await _noticias.ObtenerPorIdAsync(comando.Id, cancellationToken);
        if (noticia is null)
            return Result.Fallido($"No existe la noticia {comando.Id}.");

        _noticias.Eliminar(noticia);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result.Exitoso();
    }
}

public sealed class ComentarNoticiaManejador : IManejadorDeComando<ComentarNoticia, Result<long>>
{
    private readonly IRepositorioDeComentarios _comentarios;
    private readonly IRepositorioDeNoticias _noticias;
    private readonly IUnidadDeTrabajoColaboracion _uow;

    public ComentarNoticiaManejador(IRepositorioDeComentarios comentarios, IRepositorioDeNoticias noticias,
        IUnidadDeTrabajoColaboracion uow)
    {
        _comentarios = comentarios;
        _noticias = noticias;
        _uow = uow;
    }

    public async Task<Result<long>> ManejarAsync(ComentarNoticia comando, CancellationToken cancellationToken)
    {
        var noticia = await _noticias.ObtenerPorIdAsync(comando.IdNoticia, cancellationToken);
        if (noticia is null)
            return Result<long>.Fallido($"No existe la noticia {comando.IdNoticia}.");

        var creacion = Comentario.Crear(comando.IdNoticia, comando.Autor, comando.Avatar, comando.Texto,
            comando.Usuario, comando.Host);
        if (!creacion.EsExitoso)
            return Result<long>.Fallido(creacion.Error!);

        await _comentarios.AgregarAsync(creacion.Valor, cancellationToken);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result<long>.Exitoso(creacion.Valor.Id);
    }
}

public sealed class ObtenerNoticiaPorIdManejador : IManejadorDeConsulta<ObtenerNoticiaPorId, NoticiaDto?>
{
    private readonly IRepositorioDeNoticias _noticias;
    public ObtenerNoticiaPorIdManejador(IRepositorioDeNoticias noticias) => _noticias = noticias;

    public async Task<NoticiaDto?> ManejarAsync(ObtenerNoticiaPorId consulta, CancellationToken cancellationToken)
    {
        var noticia = await _noticias.ObtenerPorIdAsync(consulta.Id, cancellationToken);
        return noticia?.ADto();
    }
}

public sealed class ListarNoticiasManejador : IManejadorDeConsulta<ListarNoticias, ResultadoPaginado<NoticiaDto>>
{
    private readonly IRepositorioDeNoticias _noticias;
    public ListarNoticiasManejador(IRepositorioDeNoticias noticias) => _noticias = noticias;

    public async Task<ResultadoPaginado<NoticiaDto>> ManejarAsync(ListarNoticias consulta, CancellationToken cancellationToken)
    {
        var pagina = await _noticias.ListarPaginadoAsync(consulta.Parametros, cancellationToken);
        return pagina.Mapear(n => n.ADto());
    }
}

public sealed class ListarComentariosDeNoticiaManejador
    : IManejadorDeConsulta<ListarComentariosDeNoticia, IReadOnlyList<ComentarioDto>>
{
    private readonly IRepositorioDeComentarios _comentarios;
    public ListarComentariosDeNoticiaManejador(IRepositorioDeComentarios comentarios) => _comentarios = comentarios;

    public async Task<IReadOnlyList<ComentarioDto>> ManejarAsync(ListarComentariosDeNoticia consulta, CancellationToken cancellationToken)
    {
        var comentarios = await _comentarios.ListarPorNoticiaAsync(consulta.IdNoticia, cancellationToken);
        return comentarios.Select(c => c.ADto()).ToList();
    }
}
