using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace GmintaDocs.Api.Errores;

/// <summary>
/// Convierte cualquier excepción no controlada en una respuesta <c>ProblemDetails</c> uniforme
/// (RFC 7807) y la registra. Evita filtrar detalles internos al cliente en producción.
/// </summary>
public sealed class ManejadorGlobalDeExcepciones : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetails;
    private readonly ILogger<ManejadorGlobalDeExcepciones> _registro;
    private readonly IHostEnvironment _entorno;

    public ManejadorGlobalDeExcepciones(
        IProblemDetailsService problemDetails,
        ILogger<ManejadorGlobalDeExcepciones> registro,
        IHostEnvironment entorno)
    {
        _problemDetails = problemDetails;
        _registro = registro;
        _entorno = entorno;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext contexto, Exception excepcion, CancellationToken ct)
    {
        _registro.LogError(excepcion, "Excepción no controlada procesando {Metodo} {Ruta}",
            contexto.Request.Method, contexto.Request.Path);

        contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = contexto,
            Exception = excepcion,
            ProblemDetails =
            {
                Title = "Ocurrió un error inesperado.",
                Status = StatusCodes.Status500InternalServerError,
                // Solo se expone el detalle en desarrollo.
                Detail = _entorno.IsDevelopment() ? excepcion.Message : null,
            },
        });
    }
}
