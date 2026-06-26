namespace GmintaDocs.SharedKernel;

/// <summary>
/// Parámetros de paginación de una consulta de listado. Normaliza valores fuera de rango
/// (página &lt; 1, tamaño &lt; 1 o por encima del máximo) para no exponer al repositorio valores inválidos.
/// </summary>
public sealed record ParametrosDePaginacion(int Pagina = 1, int TamanoPagina = 20)
{
    public const int TamanoMaximo = 100;
    public const int TamanoPorDefecto = 20;

    public int PaginaNormalizada => Pagina < 1 ? 1 : Pagina;

    public int TamanoNormalizado =>
        TamanoPagina < 1 ? TamanoPorDefecto : (TamanoPagina > TamanoMaximo ? TamanoMaximo : TamanoPagina);

    /// <summary>Número de elementos a omitir (OFFSET) para llegar a la página solicitada.</summary>
    public int Salto => (PaginaNormalizada - 1) * TamanoNormalizado;
}

/// <summary>
/// Página de resultados de un listado: los elementos de la página más los metadatos
/// necesarios para que el cliente renderice la navegación.
/// </summary>
public sealed record ResultadoPaginado<T>(IReadOnlyList<T> Elementos, int Pagina, int TamanoPagina, int Total)
{
    public int TotalPaginas => TamanoPagina <= 0 ? 0 : (int)Math.Ceiling((double)Total / TamanoPagina);

    /// <summary>Proyecta los elementos a otro tipo conservando los metadatos de paginación (p. ej. entidad → DTO).</summary>
    public ResultadoPaginado<TDestino> Mapear<TDestino>(Func<T, TDestino> proyeccion) =>
        new(Elementos.Select(proyeccion).ToList(), Pagina, TamanoPagina, Total);
}
