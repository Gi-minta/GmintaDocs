using GmintaDocs.AdminDirectorios.Application;
using GmintaDocs.AdminDirectorios.Domain;
using GmintaDocs.Multitenancy;
using Microsoft.EntityFrameworkCore;

namespace GmintaDocs.AdminDirectorios.Infrastructure;

public sealed class RepositorioDeDirectorios : RepositorioEfBase<Directorio, long>, IRepositorioDeDirectorios
{
    public RepositorioDeDirectorios(AdminDirectoriosDbContext contexto) : base(contexto) { }

    public async Task<IReadOnlyList<Directorio>> ListarPorFormularioAsync(long idFormulario, CancellationToken cancellationToken = default)
        => await Conjunto.AsNoTracking()
            .Where(d => d.IdFormulario == idFormulario)
            .OrderBy(d => d.Nombre)
            .ToListAsync(cancellationToken);
}
