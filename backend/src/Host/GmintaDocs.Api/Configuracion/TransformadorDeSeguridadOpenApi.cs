using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace GmintaDocs.Api.Configuracion;

/// <summary>
/// Declara el esquema de seguridad <c>Bearer</c> (JWT) en el documento OpenAPI y lo exige de forma
/// global, para que las herramientas (Scalar, Swagger UI, .http) permitan probar endpoints protegidos.
/// </summary>
public sealed class TransformadorDeSeguridadOpenApi : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument documento, OpenApiDocumentTransformerContext contexto, CancellationToken cancellationToken)
    {
        documento.Components ??= new OpenApiComponents();
        documento.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        documento.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Autenticación JWT. Indique el token emitido por 'api/auth/login'.",
        };

        documento.Security ??= [];
        documento.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", documento)] = new List<string>(),
        });

        return Task.CompletedTask;
    }
}
