/** Control de navegación de páginas para listados paginados. Se oculta si solo hay una página. */
export function Paginacion({
  pagina,
  totalPaginas,
  total,
  onCambiar,
}: {
  pagina: number
  totalPaginas: number
  total: number
  onCambiar: (pagina: number) => void
}) {
  if (totalPaginas <= 1) return null

  return (
    <div className="paginacion">
      <button className="enlace" disabled={pagina <= 1} onClick={() => onCambiar(pagina - 1)}>
        ‹ Anterior
      </button>
      <span className="sutil">
        Página {pagina} de {totalPaginas} · {total} en total
      </span>
      <button className="enlace" disabled={pagina >= totalPaginas} onClick={() => onCambiar(pagina + 1)}>
        Siguiente ›
      </button>
    </div>
  )
}
