/** Botón de borrado con confirmación previa, reutilizado en las páginas de gestión. */
export function BotonEliminar({
  onEliminar,
  etiqueta = 'Eliminar',
  confirmacion = '¿Eliminar este elemento? Esta acción no se puede deshacer.',
}: {
  onEliminar: () => void | Promise<void>
  etiqueta?: string
  confirmacion?: string
}) {
  function alHacerClic() {
    if (window.confirm(confirmacion)) void onEliminar()
  }

  return (
    <button type="button" className="enlace peligro" onClick={alHacerClic}>
      {etiqueta}
    </button>
  )
}
