using GmintaDocs.CQRS;
using GmintaDocs.Reportes.Domain;
using GmintaDocs.SharedKernel;

namespace GmintaDocs.Reportes.Application;

// ---- Comandos ----
public sealed record CrearCategoriaReporte(string Codigo, string Categoria, string? Descripcion,
    string Usuario, string Host) : IComando<Result<long>>;

public sealed record CrearReporte(long IdCategoria, string Codigo, string Reporte, string Url,
    string? Descripcion, string Usuario, string Host) : IComando<Result<long>>;

public sealed record ActualizarReporte(long Id, string Reporte, string Url, string? Descripcion) : IComando<Result>;
public sealed record EliminarReporte(long Id) : IComando<Result>;

// ---- Consultas ----
public sealed record ListarCategoriasReporte(ParametrosDePaginacion Parametros) : IConsulta<ResultadoPaginado<CategoriaReporteDto>>;
public sealed record ListarReportesPorCategoria(long IdCategoria) : IConsulta<IReadOnlyList<ReporteDto>>;

// ---- Manejadores ----
public sealed class CrearCategoriaReporteManejador : IManejadorDeComando<CrearCategoriaReporte, Result<long>>
{
    private readonly IRepositorioDeCategoriasReporte _categorias;
    private readonly IUnidadDeTrabajoReportes _uow;

    public CrearCategoriaReporteManejador(IRepositorioDeCategoriasReporte categorias, IUnidadDeTrabajoReportes uow)
    {
        _categorias = categorias;
        _uow = uow;
    }

    public async Task<Result<long>> ManejarAsync(CrearCategoriaReporte comando, CancellationToken cancellationToken)
    {
        var creacion = CategoriaReporte.Crear(comando.Codigo, comando.Categoria, comando.Descripcion,
            comando.Usuario, comando.Host);
        if (!creacion.EsExitoso)
            return Result<long>.Fallido(creacion.Error!);

        await _categorias.AgregarAsync(creacion.Valor, cancellationToken);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result<long>.Exitoso(creacion.Valor.Id);
    }
}

public sealed class CrearReporteManejador : IManejadorDeComando<CrearReporte, Result<long>>
{
    private readonly IRepositorioDeReportes _reportes;
    private readonly IRepositorioDeCategoriasReporte _categorias;
    private readonly IUnidadDeTrabajoReportes _uow;

    public CrearReporteManejador(IRepositorioDeReportes reportes, IRepositorioDeCategoriasReporte categorias,
        IUnidadDeTrabajoReportes uow)
    {
        _reportes = reportes;
        _categorias = categorias;
        _uow = uow;
    }

    public async Task<Result<long>> ManejarAsync(CrearReporte comando, CancellationToken cancellationToken)
    {
        var categoria = await _categorias.ObtenerPorIdAsync(comando.IdCategoria, cancellationToken);
        if (categoria is null)
            return Result<long>.Fallido($"No existe la categoría {comando.IdCategoria}.");

        var creacion = ReporteSsrs.Crear(comando.IdCategoria, comando.Codigo, comando.Reporte, comando.Url,
            comando.Descripcion, comando.Usuario, comando.Host);
        if (!creacion.EsExitoso)
            return Result<long>.Fallido(creacion.Error!);

        await _reportes.AgregarAsync(creacion.Valor, cancellationToken);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result<long>.Exitoso(creacion.Valor.Id);
    }
}

public sealed class ActualizarReporteManejador : IManejadorDeComando<ActualizarReporte, Result>
{
    private readonly IRepositorioDeReportes _reportes;
    private readonly IUnidadDeTrabajoReportes _uow;

    public ActualizarReporteManejador(IRepositorioDeReportes reportes, IUnidadDeTrabajoReportes uow)
    {
        _reportes = reportes; _uow = uow;
    }

    public async Task<Result> ManejarAsync(ActualizarReporte comando, CancellationToken cancellationToken)
    {
        var reporte = await _reportes.ObtenerPorIdAsync(comando.Id, cancellationToken);
        if (reporte is null)
            return Result.Fallido($"No existe el reporte {comando.Id}.");

        var resultado = reporte.Editar(comando.Reporte, comando.Url, comando.Descripcion);
        if (!resultado.EsExitoso)
            return resultado;

        _reportes.Actualizar(reporte);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result.Exitoso();
    }
}

public sealed class EliminarReporteManejador : IManejadorDeComando<EliminarReporte, Result>
{
    private readonly IRepositorioDeReportes _reportes;
    private readonly IUnidadDeTrabajoReportes _uow;

    public EliminarReporteManejador(IRepositorioDeReportes reportes, IUnidadDeTrabajoReportes uow)
    {
        _reportes = reportes; _uow = uow;
    }

    public async Task<Result> ManejarAsync(EliminarReporte comando, CancellationToken cancellationToken)
    {
        var reporte = await _reportes.ObtenerPorIdAsync(comando.Id, cancellationToken);
        if (reporte is null)
            return Result.Fallido($"No existe el reporte {comando.Id}.");

        _reportes.Eliminar(reporte);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result.Exitoso();
    }
}

public sealed class ListarCategoriasReporteManejador
    : IManejadorDeConsulta<ListarCategoriasReporte, ResultadoPaginado<CategoriaReporteDto>>
{
    private readonly IRepositorioDeCategoriasReporte _categorias;
    public ListarCategoriasReporteManejador(IRepositorioDeCategoriasReporte categorias) => _categorias = categorias;

    public async Task<ResultadoPaginado<CategoriaReporteDto>> ManejarAsync(ListarCategoriasReporte consulta, CancellationToken cancellationToken)
    {
        var pagina = await _categorias.ListarPaginadoAsync(consulta.Parametros, cancellationToken);
        return pagina.Mapear(c => c.ADto());
    }
}

public sealed class ListarReportesPorCategoriaManejador
    : IManejadorDeConsulta<ListarReportesPorCategoria, IReadOnlyList<ReporteDto>>
{
    private readonly IRepositorioDeReportes _reportes;
    public ListarReportesPorCategoriaManejador(IRepositorioDeReportes reportes) => _reportes = reportes;

    public async Task<IReadOnlyList<ReporteDto>> ManejarAsync(ListarReportesPorCategoria consulta, CancellationToken cancellationToken)
    {
        var reportes = await _reportes.ListarPorCategoriaAsync(consulta.IdCategoria, cancellationToken);
        return reportes.Select(r => r.ADto()).ToList();
    }
}
