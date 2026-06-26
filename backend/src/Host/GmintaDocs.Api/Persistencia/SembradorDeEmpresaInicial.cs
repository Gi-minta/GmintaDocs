using GmintaDocs.CQRS;
using GmintaDocs.Organizacion.Application;
using GmintaDocs.Organizacion.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace GmintaDocs.Api.Persistencia;

/// <summary>
/// Siembra (si no existe) una empresa inicial en la base MAESTRA y aprovisiona su base de datos
/// dedicada. Resuelve el bootstrap de un sistema recién instalado: sin esto, el admin puede
/// iniciar sesión (el token solo estampa <c>id_empresa</c>) pero toda página de negocio falla con
/// 500 porque la base <c>gmintadocs_empresa_{id}</c> aún no existe. Es idempotente —si ya hay una
/// empresa registrada, solo (re)aprovisiona su base, lo que a su vez es idempotente vía MigrateAsync—.
/// Opt-in por configuración (<c>Persistencia:AprovisionarEmpresaInicial</c>).
/// </summary>
public static class SembradorDeEmpresaInicial
{
    public static async Task AprovisionarEmpresaInicialAsync(
        this IServiceProvider servicios, IConfiguration configuracion, CancellationToken ct = default)
    {
        if (!configuracion.GetValue<bool>("Persistencia:AprovisionarEmpresaInicial"))
            return;

        await using var scope = servicios.CreateAsyncScope();
        var sp = scope.ServiceProvider;
        var contexto = sp.GetRequiredService<OrganizacionDbContext>();
        var registro = sp.GetRequiredService<ILoggerFactory>().CreateLogger("SembradorDeEmpresaInicial");

        // 1) Empresa inicial: la primera registrada o, si no hay ninguna, se crea.
        var primera = await contexto.Empresas.OrderBy(e => e.Id).FirstOrDefaultAsync(ct);
        long idEmpresa;
        if (primera is not null)
        {
            idEmpresa = primera.Id;
        }
        else
        {
            var razonSocial = configuracion["Organizacion:EmpresaInicial:RazonSocial"] ?? "Empresa Demo";
            var nit = configuracion["Organizacion:EmpresaInicial:Nit"] ?? "900000000-1";

            var despachador = sp.GetRequiredService<IDespachador>();
            var creacion = await despachador.EnviarAsync(new CrearEmpresa(razonSocial, nit, "seed", "localhost"), ct);
            if (!creacion.EsExitoso)
            {
                registro.LogWarning("No se sembró la empresa inicial: {Error}", creacion.Error);
                return;
            }

            idEmpresa = creacion.Valor;
            registro.LogInformation("Empresa inicial '{Razon}' (id {Id}) sembrada en la base maestra.", razonSocial, idEmpresa);
        }

        // 2) Aprovisiona (crea + migra) la base de datos dedicada de esa empresa. Idempotente.
        var aprovisionador = sp.GetRequiredService<AprovisionadorDeEmpresa>();
        await aprovisionador.AprovisionarAsync(idEmpresa, ct);
        registro.LogInformation("Base de datos de la empresa {Id} aprovisionada.", idEmpresa);
    }
}
