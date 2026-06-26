using GmintaDocs.Plantillas.Domain;
using GmintaDocs.SharedKernel;

namespace GmintaDocs.Plantillas.Application;

/// <summary>Unidad de trabajo del módulo (evita colisión de DI entre los DbContext por módulo).</summary>
public interface IUnidadDeTrabajoPlantillas : IUnidadDeTrabajo { }

public interface IRepositorioDePlantillas : IRepositorio<Plantilla, int>
{
    Task<Plantilla?> ObtenerPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);
    Task<ResultadoPaginado<Plantilla>> ListarPaginadoAsync(ParametrosDePaginacion parametros, CancellationToken cancellationToken = default);
}

public interface IRepositorioDePlantillasFormato : IRepositorio<PlantillaFormato, int>
{
    Task<IReadOnlyList<PlantillaFormato>> ListarAsync(CancellationToken cancellationToken = default);
}

public sealed record PlantillaDto(int Id, string Codigo, string Nombre, string Contenido);
public sealed record PlantillaFormatoDto(int Id, string Codigo, string Nombre, int Estado, long IdFormulario);

/// <summary>Mapeos explícitos dominio → DTO (sin AutoMapper, por decisión de stack).</summary>
public static class MapeosPlantillas
{
    public static PlantillaDto ADto(this Plantilla p) => new(p.Id, p.Codigo, p.Nombre, p.Contenido);

    public static PlantillaFormatoDto ADto(this PlantillaFormato p) =>
        new(p.Id, p.Codigo, p.Nombre, p.Estado, p.IdFormulario);
}
