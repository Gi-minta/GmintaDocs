namespace GmintaDocs.SharedKernel;

/// <summary>
/// Resultado de una operación de negocio, sin lanzar excepciones para flujos esperados.
/// </summary>
public class Result
{
    public bool EsExitoso { get; }
    public string? Error { get; }

    protected Result(bool esExitoso, string? error)
    {
        EsExitoso = esExitoso;
        Error = error;
    }

    public static Result Exitoso() => new(true, null);
    public static Result Fallido(string error) => new(false, error);
}

/// <summary>
/// Resultado de una operación de negocio que además devuelve un valor.
/// </summary>
public class Result<TValue> : Result
{
    private readonly TValue? _valor;

    protected Result(TValue? valor, bool esExitoso, string? error) : base(esExitoso, error)
    {
        _valor = valor;
    }

    public TValue Valor => EsExitoso
        ? _valor!
        : throw new InvalidOperationException("No se puede acceder al valor de un resultado fallido.");

    public static Result<TValue> Exitoso(TValue valor) => new(valor, true, null);
    public static new Result<TValue> Fallido(string error) => new(default, false, error);
}
