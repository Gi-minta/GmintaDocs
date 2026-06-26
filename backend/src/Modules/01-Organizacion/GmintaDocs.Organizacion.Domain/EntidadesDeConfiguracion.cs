using GmintaDocs.SharedKernel;

namespace GmintaDocs.Organizacion.Domain;

/// <summary>Asignación de un rol a una empresa (tabla roles_empresa, clave compuesta).</summary>
public sealed class RolEmpresa
{
    public long IdEmpresa { get; private set; }
    public string IdRol { get; private set; } = string.Empty;

    private RolEmpresa() { }

    public RolEmpresa(long idEmpresa, string idRol)
    {
        IdEmpresa = idEmpresa;
        IdRol = idRol;
    }
}

/// <summary>Consecutivo de radicado por formulario a nivel de empresa.</summary>
public sealed class RadicadoEmpresa : AggregateRoot<long>
{
    public long IdFormulario { get; private set; }
    public long IdEmpresa { get; private set; }
    public string Radicado { get; private set; } = string.Empty;

    private RadicadoEmpresa() { }

    public RadicadoEmpresa(long idFormulario, long idEmpresa, string radicado)
    {
        IdFormulario = idFormulario;
        IdEmpresa = idEmpresa;
        Radicado = radicado;
    }
}

/// <summary>Consecutivo de radicado por formulario a nivel de sucursal.</summary>
public sealed class RadicadoSucursal : AggregateRoot<long>
{
    public long IdFormulario { get; private set; }
    public long IdSucursal { get; private set; }
    public string Radicado { get; private set; } = string.Empty;

    private RadicadoSucursal() { }

    public RadicadoSucursal(long idFormulario, long idSucursal, string radicado)
    {
        IdFormulario = idFormulario;
        IdSucursal = idSucursal;
        Radicado = radicado;
    }
}

/// <summary>Parámetro global de configuración del sistema.</summary>
public sealed class Parametro : AggregateRoot<int>
{
    public int IdLogico { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public string Valor { get; private set; } = string.Empty;
    public string? Grupo { get; private set; }

    private Parametro() { }

    public Parametro(int idLogico, string descripcion, string valor, string? grupo)
    {
        IdLogico = idLogico;
        Descripcion = descripcion;
        Valor = valor;
        Grupo = grupo;
    }

    public void CambiarValor(string valor) => Valor = valor;
}

/// <summary>Día feriado para el cálculo de plazos hábiles del workflow.</summary>
public sealed class Feriado : AggregateRoot<long>
{
    public DateTime FechaFeriado { get; private set; }
    public string? Descripcion { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Host { get; private set; } = string.Empty;

    private Feriado() { }

    public Feriado(DateTime fechaFeriado, string? descripcion, string usuario, string host)
    {
        FechaFeriado = fechaFeriado;
        Descripcion = descripcion;
        Usuario = usuario;
        Host = host;
        Fecha = DateTime.UtcNow;
    }
}
