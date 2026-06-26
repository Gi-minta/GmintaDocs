namespace GmintaDocs.CQRS;

/// <summary>Marca un comando: una intención de cambio de estado.</summary>
public interface IComando<TRespuesta> { }

/// <summary>Marca una consulta: una intención de lectura, sin efectos secundarios.</summary>
public interface IConsulta<TRespuesta> { }

/// <summary>Maneja la ejecución de un comando concreto.</summary>
public interface IManejadorDeComando<TComando, TRespuesta> where TComando : IComando<TRespuesta>
{
    Task<TRespuesta> ManejarAsync(TComando comando, CancellationToken cancellationToken);
}

/// <summary>Maneja la ejecución de una consulta concreta.</summary>
public interface IManejadorDeConsulta<TConsulta, TRespuesta> where TConsulta : IConsulta<TRespuesta>
{
    Task<TRespuesta> ManejarAsync(TConsulta consulta, CancellationToken cancellationToken);
}
