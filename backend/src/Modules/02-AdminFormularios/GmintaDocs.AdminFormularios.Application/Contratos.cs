using GmintaDocs.AdminFormularios.Domain;
using GmintaDocs.SharedKernel;

namespace GmintaDocs.AdminFormularios.Application;

/// <summary>Unidad de trabajo del módulo (evita colisión de DI entre los DbContext por módulo).</summary>
public interface IUnidadDeTrabajoAdminFormularios : IUnidadDeTrabajo { }

public interface IRepositorioDeFormularios : IRepositorio<Formulario, long>
{
    Task<Formulario?> ObtenerPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);
    Task<ResultadoPaginado<Formulario>> ListarPaginadoAsync(ParametrosDePaginacion parametros, CancellationToken cancellationToken = default);
}

public interface IRepositorioDeCampos : IRepositorio<Campo, long>
{
    Task<IReadOnlyList<Campo>> ListarPorFormularioAsync(long idFormulario, CancellationToken cancellationToken = default);
}

public sealed record FormularioDto(long Id, string Codigo, string Tabla, string Nombre, string? Descripcion, short Estado);

public sealed record CampoDto(long Id, long IdFormulario, int Orden, string Nombre, string Columna, short TipoDato, bool Requerido);

/// <summary>Mapeos explícitos dominio → DTO (sin AutoMapper, por decisión de stack).</summary>
public static class MapeosAdminFormularios
{
    public static FormularioDto ADto(this Formulario f) =>
        new(f.Id, f.Codigo, f.Tabla, f.Nombre, f.Descripcion, f.Estado);

    public static CampoDto ADto(this Campo c) =>
        new(c.Id, c.IdFormulario, c.Orden, c.Nombre, c.Columna, c.TipoDato, c.Requerido);
}
