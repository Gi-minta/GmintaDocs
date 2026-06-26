import { useCallback, useEffect, useState } from 'react'
import { api, ErrorApi } from '../api/cliente'

/**
 * Carga una lista desde la API y la mantiene en estado.
 * El efecto hace `await` antes de tocar el estado (regla `set-state-in-effect`);
 * `recargar` queda disponible para volver a pedir los datos tras una mutación.
 */
export function useLista<T>(ruta: string) {
  const [datos, setDatos] = useState<T[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const recargar = useCallback(async () => {
    try {
      const resultado = await api.get<T[]>(ruta)
      setDatos(resultado)
      setError(null)
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudieron cargar los datos.')
    } finally {
      setCargando(false)
    }
  }, [ruta])

  useEffect(() => {
    let activo = true
    ;(async () => {
      try {
        const resultado = await api.get<T[]>(ruta)
        if (activo) {
          setDatos(resultado)
          setError(null)
        }
      } catch (e) {
        if (activo) setError(e instanceof ErrorApi ? e.message : 'No se pudieron cargar los datos.')
      } finally {
        if (activo) setCargando(false)
      }
    })()
    return () => {
      activo = false
    }
  }, [ruta])

  return { datos, cargando, error, recargar, setError }
}
