using GmintaDocs.AdminDirectorios.Domain;
using GmintaDocs.SharedKernel;

namespace GmintaDocs.AdminDirectorios.Application;

/// <summary>Unidad de trabajo del módulo (evita colisión de DI entre los DbContext por módulo).</summary>
public interface IUnidadDeTrabajoAdminDirectorios : IUnidadDeTrabajo { }

public interface IRepositorioDeDirectorios : IRepositorio<Directorio, long>
{
    Task<IReadOnlyList<Directorio>> ListarPorFormularioAsync(long idFormulario, CancellationToken cancellationToken = default);
}

public sealed record DirectorioDto(long Id, long IdFormulario, long IdDirectorioPadre, string Codigo,
    string Nombre, int IdEstado);

/// <summary>Mapeos explícitos dominio → DTO (sin AutoMapper, por decisión de stack).</summary>
public static class MapeosAdminDirectorios
{
    public static DirectorioDto ADto(this Directorio d) =>
        new(d.Id, d.IdFormulario, d.IdDirectorioPadre, d.Codigo, d.Nombre, d.IdEstado);
}
