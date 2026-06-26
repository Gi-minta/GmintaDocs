using System.Text;
using GmintaDocs.AdminDirectorios.Infrastructure;
using GmintaDocs.AdminFormularios.Infrastructure;
using GmintaDocs.Api.Multitenancy;
using GmintaDocs.Api.Persistencia;
using GmintaDocs.Api.Seguridad;
using GmintaDocs.Api.Configuracion;
using GmintaDocs.Api.Errores;
using GmintaDocs.Colaboracion.Infrastructure;
using GmintaDocs.CQRS;
using GmintaDocs.GestionDocumental.Infrastructure;
using GmintaDocs.Identidad.Infrastructure;
using GmintaDocs.Multitenancy;
using GmintaDocs.Organizacion.Infrastructure;
using Scalar.AspNetCore;
using GmintaDocs.Plantillas.Infrastructure;
using GmintaDocs.Reportes.Infrastructure;
using GmintaDocs.Tareas.Infrastructure;
using GmintaDocs.Workflow.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi(opciones => opciones.AddDocumentTransformer<TransformadorDeSeguridadOpenApi>());

// Manejo de errores uniforme (RFC 7807): ProblemDetails + manejador global de excepciones.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorGlobalDeExcepciones>();

// CQRS propio (sin MediatR): despachador de comandos y consultas.
builder.Services.AddScoped<IDespachador, Despachador>();

// Autenticación: JWT Bearer firmado con HMAC-SHA256. La empresa activa viaja en el claim "id_empresa".
var opcionesJwt = builder.Configuration.GetSection(OpcionesDeJwt.Seccion).Get<OpcionesDeJwt>()
    ?? throw new InvalidOperationException("Falta la sección de configuración 'Jwt'.");
if (string.IsNullOrWhiteSpace(opcionesJwt.ClaveSecreta) || opcionesJwt.ClaveSecreta.Length < 32)
    throw new InvalidOperationException("La 'Jwt:ClaveSecreta' debe tener al menos 32 caracteres.");

builder.Services.Configure<OpcionesDeJwt>(builder.Configuration.GetSection(OpcionesDeJwt.Seccion));
builder.Services.AddSingleton<GeneradorDeTokensJwt>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = opcionesJwt.Emisor,
            ValidAudience = opcionesJwt.Audiencia,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opcionesJwt.ClaveSecreta)),
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });
builder.Services.AddAuthorization(PoliticasDeAutorizacion.Registrar);

// CORS: permite que el SPA (desplegado aparte, sin el proxy de Vite) consuma la API.
// Orígenes permitidos desde 'Cors:OrigenesPermitidos'; vacío = no se habilita ningún origen externo.
const string PoliticaCors = "SpaGmintaDocs";
var origenesCors = builder.Configuration.GetSection("Cors:OrigenesPermitidos").Get<string[]>() ?? [];
builder.Services.AddCors(opciones =>
    opciones.AddPolicy(PoliticaCors, politica =>
    {
        if (origenesCors.Length > 0)
            politica.WithOrigins(origenesCors).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    }));

// Multitenancy: contexto de tenant scoped + resolución de cadena de conexión por empresa.
builder.Services.AgregarMultitenancy();

// Base de datos MAESTRA/control: registro de tenants e identidad.
var cadenaMaestra = builder.Configuration.GetConnectionString("Maestro")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'Maestro' (base de datos de control).");

builder.Services.AgregarModuloIdentidad(cadenaMaestra);
builder.Services.AgregarModuloOrganizacion(cadenaMaestra);

// Módulos de negocio: cada uno usa la base de datos dedicada de la empresa activa (resuelta en runtime).
builder.Services.AgregarModuloAdminFormularios();
builder.Services.AgregarModuloAdminDirectorios();
builder.Services.AgregarModuloWorkflow();
builder.Services.AgregarModuloTareas();
builder.Services.AgregarModuloGestionDocumental();
builder.Services.AgregarModuloPlantillas();
builder.Services.AgregarModuloReportes();
builder.Services.AgregarModuloColaboracion();

// Aprovisionamiento de bases de datos por empresa (crear + migrar la BD dedicada de un tenant).
builder.Services.AddSingleton<AprovisionadorDeEmpresa>();

var app = builder.Build();

// Migración de la BD maestra al arrancar (opt-in por configuración; requiere PostgreSQL disponible).
// Tras migrar se siembran los roles del sistema y el administrador inicial (idempotente).
if (app.Configuration.GetValue<bool>("Persistencia:MigrarMaestraAlArrancar"))
{
    await app.Services.AplicarMigracionesMaestrasAsync();
    await app.Services.SembrarIdentidadAsync(app.Configuration);
}

// Primer middleware del pipeline: captura excepciones no controladas y responde ProblemDetails.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // UI interactiva para explorar y probar la API (incluye botón de autenticación Bearer) en /scalar.
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors(PoliticaCors);

app.UseAuthentication();

// Resuelve la empresa activa (cabecera o claim del token) antes de tocar cualquier DbContext por empresa.
app.UseMiddleware<MiddlewareDeTenant>();

app.UseAuthorization();

app.MapControllers();

app.Run();
