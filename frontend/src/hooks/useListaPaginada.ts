import { useCallback, useEffect, useState } from 'react'
import { api, ErrorApi } from '../api/cliente'
import type { ResultadoPaginado } from '../tipos/modelos'

/**
 * Carga una página de una lista desde un endpoint paginado (`?pagina=&tamano=`).
 * Mantiene el patrón de `useLista`: el efecto hace `await` antes de tocar el estado
 * (regla `react-hooks/set-state-in-effect`). `recargar` re-pide la página actual tras una mutación.
 */
export function useListaPaginada<T>(rutaBase: string, tamano = 20) {
  const [pagina, setPagina] = useState(1)
  const [datos, setDatos] = useState<T[]>([])
  const [total, setTotal] = useState(0)
  const [totalPaginas, setTotalPaginas] = useState(0)
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const separador = rutaBase.includes('?') ? '&' : '?'
  const ruta = `${rutaBase}${separador}pagina=${pagina}&tamano=${tamano}`

  const aplicar = useCallback((r: ResultadoPaginado<T>) => {
    setDatos(r.elementos)
    setTotal(r.total)
    setTotalPaginas(r.totalPaginas)
    setError(null)
  }, [])

  const recargar = useCallback(async () => {
    try {
      aplicar(await api.get<ResultadoPaginado<T>>(ruta))
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudieron cargar los datos.')
    } finally {
      setCargando(false)
    }
  }, [ruta, aplicar])

  useEffect(() => {
    let activo = true
    ;(async () => {
      try {
        const r = await api.get<ResultadoPaginado<T>>(ruta)
        if (activo) aplicar(r)
      } catch (e) {
        if (activo) setError(e instanceof ErrorApi ? e.message : 'No se pudieron cargar los datos.')
      } finally {
        if (activo) setCargando(false)
      }
    })()
    return () => {
      activo = false
    }
  }, [ruta, aplicar])

  return { datos, cargando, error, recargar, setError, pagina, setPagina, total, totalPaginas }
}
