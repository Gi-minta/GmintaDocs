using Microsoft.Extensions.DependencyInjection;

namespace GmintaDocs.CQRS;

/// <summary>
/// Despachador propio de comandos y consultas (sustituye a MediatR).
/// Resuelve el manejador correspondiente vía DI y lo invoca.
/// </summary>
public class Despachador : IDespachador
{
    private readonly IServiceProvider _serviceProvider;

    public Despachador(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TRespuesta> EnviarAsync<TRespuesta>(IComando<TRespuesta> comando, CancellationToken cancellationToken = default)
    {
        var tipoManejador = typeof(IManejadorDeComando<,>).MakeGenericType(comando.GetType(), typeof(TRespuesta));
        dynamic manejador = _serviceProvider.GetRequiredService(tipoManejador);
        return await manejador.ManejarAsync((dynamic)comando, cancellationToken);
    }

    public async Task<TRespuesta> ConsultarAsync<TRespuesta>(IConsulta<TRespuesta> consulta, CancellationToken cancellationToken = default)
    {
        var tipoManejador = typeof(IManejadorDeConsulta<,>).MakeGenericType(consulta.GetType(), typeof(TRespuesta));
        dynamic manejador = _serviceProvider.GetRequiredService(tipoManejador);
        return await manejador.ManejarAsync((dynamic)consulta, cancellationToken);
    }
}

public interface IDespachador
{
    Task<TRespuesta> EnviarAsync<TRespuesta>(IComando<TRespuesta> comando, CancellationToken cancellationToken = default);
    Task<TRespuesta> ConsultarAsync<TRespuesta>(IConsulta<TRespuesta> consulta, CancellationToken cancellationToken = default);
}
