using GmintaDocs.CQRS;
using GmintaDocs.Plantillas.Domain;
using GmintaDocs.SharedKernel;

namespace GmintaDocs.Plantillas.Application;

// ---- Comandos ----
public sealed record CrearPlantilla(string Codigo, string Nombre, string? Contenido) : IComando<Result<int>>;
public sealed record ActualizarPlantilla(int Id, string Nombre, string? Contenido) : IComando<Result>;
public sealed record EliminarPlantilla(int Id) : IComando<Result>;
public sealed record CrearPlantillaFormato(string Codigo, string Nombre, string? FormatoHtml, long IdFormulario,
    string Usuario, string Host) : IComando<Result<int>>;

// ---- Consultas ----
public sealed record ObtenerPlantillaPorId(int Id) : IConsulta<PlantillaDto?>;
public sealed record ListarPlantillas(ParametrosDePaginacion Parametros) : IConsulta<ResultadoPaginado<PlantillaDto>>;
public sealed record ListarPlantillasFormato : IConsulta<IReadOnlyList<PlantillaFormatoDto>>;

// ---- Manejadores ----
public sealed class CrearPlantillaManejador : IManejadorDeComando<CrearPlantilla, Result<int>>
{
    private readonly IRepositorioDePlantillas _plantillas;
    private readonly IUnidadDeTrabajoPlantillas _uow;

    public CrearPlantillaManejador(IRepositorioDePlantillas plantillas, IUnidadDeTrabajoPlantillas uow)
    {
        _plantillas = plantillas;
        _uow = uow;
    }

    public async Task<Result<int>> ManejarAsync(CrearPlantilla comando, CancellationToken cancellationToken)
    {
        var existente = await _plantillas.ObtenerPorCodigoAsync(comando.Codigo, cancellationToken);
        if (existente is not null)
            return Result<int>.Fallido($"Ya existe una plantilla con el código '{comando.Codigo}'.");

        var creacion = Plantilla.Crear(comando.Codigo, comando.Nombre, comando.Contenido);
        if (!creacion.EsExitoso)
            return Result<int>.Fallido(creacion.Error!);

        await _plantillas.AgregarAsync(creacion.Valor, cancellationToken);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result<int>.Exitoso(creacion.Valor.Id);
    }
}

public sealed class ActualizarPlantillaManejador : IManejadorDeComando<ActualizarPlantilla, Result>
{
    private readonly IRepositorioDePlantillas _plantillas;
    private readonly IUnidadDeTrabajoPlantillas _uow;

    public ActualizarPlantillaManejador(IRepositorioDePlantillas plantillas, IUnidadDeTrabajoPlantillas uow)
    {
        _plantillas = plantillas; _uow = uow;
    }

    public async Task<Result> ManejarAsync(ActualizarPlantilla comando, CancellationToken cancellationToken)
    {
        var plantilla = await _plantillas.ObtenerPorIdAsync(comando.Id, cancellationToken);
        if (plantilla is null)
            return Result.Fallido($"No existe la plantilla {comando.Id}.");

        var resultado = plantilla.Editar(comando.Nombre, comando.Contenido);
        if (!resultado.EsExitoso)
            return resultado;

        _plantillas.Actualizar(plantilla);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result.Exitoso();
    }
}

public sealed class EliminarPlantillaManejador : IManejadorDeComando<EliminarPlantilla, Result>
{
    private readonly IRepositorioDePlantillas _plantillas;
    private readonly IUnidadDeTrabajoPlantillas _uow;

    public EliminarPlantillaManejador(IRepositorioDePlantillas plantillas, IUnidadDeTrabajoPlantillas uow)
    {
        _plantillas = plantillas; _uow = uow;
    }

    public async Task<Result> ManejarAsync(EliminarPlantilla comando, CancellationToken cancellationToken)
    {
        var plantilla = await _plantillas.ObtenerPorIdAsync(comando.Id, cancellationToken);
        if (plantilla is null)
            return Result.Fallido($"No existe la plantilla {comando.Id}.");

        _plantillas.Eliminar(plantilla);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result.Exitoso();
    }
}

public sealed class CrearPlantillaFormatoManejador : IManejadorDeComando<CrearPlantillaFormato, Result<int>>
{
    private readonly IRepositorioDePlantillasFormato _formatos;
    private readonly IUnidadDeTrabajoPlantillas _uow;

    public CrearPlantillaFormatoManejador(IRepositorioDePlantillasFormato formatos, IUnidadDeTrabajoPlantillas uow)
    {
        _formatos = formatos;
        _uow = uow;
    }

    public async Task<Result<int>> ManejarAsync(CrearPlantillaFormato comando, CancellationToken cancellationToken)
    {
        var creacion = PlantillaFormato.Crear(comando.Codigo, comando.Nombre, comando.FormatoHtml,
            comando.IdFormulario, comando.Usuario, comando.Host);
        if (!creacion.EsExitoso)
            return Result<int>.Fallido(creacion.Error!);

        await _formatos.AgregarAsync(creacion.Valor, cancellationToken);
        await _uow.GuardarCambiosAsync(cancellationToken);
        return Result<int>.Exitoso(creacion.Valor.Id);
    }
}

public sealed class ObtenerPlantillaPorIdManejador : IManejadorDeConsulta<ObtenerPlantillaPorId, PlantillaDto?>
{
    private readonly IRepositorioDePlantillas _plantillas;
    public ObtenerPlantillaPorIdManejador(IRepositorioDePlantillas plantillas) => _plantillas = plantillas;

    public async Task<PlantillaDto?> ManejarAsync(ObtenerPlantillaPorId consulta, CancellationToken cancellationToken)
    {
        var plantilla = await _plantillas.ObtenerPorIdAsync(consulta.Id, cancellationToken);
        return plantilla?.ADto();
    }
}

public sealed class ListarPlantillasManejador : IManejadorDeConsulta<ListarPlantillas, ResultadoPaginado<PlantillaDto>>
{
    private readonly IRepositorioDePlantillas _plantillas;
    public ListarPlantillasManejador(IRepositorioDePlantillas plantillas) => _plantillas = plantillas;

    public async Task<ResultadoPaginado<PlantillaDto>> ManejarAsync(ListarPlantillas consulta, CancellationToken cancellationToken)
    {
        var pagina = await _plantillas.ListarPaginadoAsync(consulta.Parametros, cancellationToken);
        return pagina.Mapear(p => p.ADto());
    }
}

public sealed class ListarPlantillasFormatoManejador
    : IManejadorDeConsulta<ListarPlantillasFormato, IReadOnlyList<PlantillaFormatoDto>>
{
    private readonly IRepositorioDePlantillasFormato _formatos;
    public ListarPlantillasFormatoManejador(IRepositorioDePlantillasFormato formatos) => _formatos = formatos;

    public async Task<IReadOnlyList<PlantillaFormatoDto>> ManejarAsync(ListarPlantillasFormato consulta, CancellationToken cancellationToken)
    {
        var formatos = await _formatos.ListarAsync(cancellationToken);
        return formatos.Select(f => f.ADto()).ToList();
    }
}
