// Cliente HTTP mínimo sobre fetch. Adjunta el token JWT y centraliza el manejo de errores.

const BASE = (import.meta.env.VITE_API_URL as string | undefined) ?? '/api'

let tokenActual: string | null = null
let alExpirar: (() => void) | null = null

export function establecerToken(token: string | null): void {
  tokenActual = token
}

export function alPerderSesion(callback: () => void): void {
  alExpirar = callback
}

export class ErrorApi extends Error {
  readonly estado: number

  constructor(estado: number, mensaje: string) {
    super(mensaje)
    this.name = 'ErrorApi'
    this.estado = estado
  }
}

async function pedir<T>(ruta: string, opciones: RequestInit = {}): Promise<T> {
  const cabeceras = new Headers(opciones.headers)
  if (opciones.body) cabeceras.set('Content-Type', 'application/json')
  if (tokenActual) cabeceras.set('Authorization', `Bearer ${tokenActual}`)

  const respuesta = await fetch(`${BASE}${ruta}`, { ...opciones, headers: cabeceras })

  if (respuesta.status === 401) {
    alExpirar?.()
    throw new ErrorApi(401, 'Sesión expirada o credenciales inválidas.')
  }

  if (!respuesta.ok) {
    throw new ErrorApi(respuesta.status, await leerError(respuesta))
  }

  if (respuesta.status === 204) return undefined as T
  const texto = await respuesta.text()
  return (texto ? JSON.parse(texto) : undefined) as T
}

async function leerError(respuesta: Response): Promise<string> {
  try {
    const cuerpo = await respuesta.json()
    return cuerpo.error ?? cuerpo.detail ?? cuerpo.title ?? `Error ${respuesta.status}`
  } catch {
    return `Error ${respuesta.status}`
  }
}

export const api = {
  get: <T>(ruta: string) => pedir<T>(ruta),
  post: <T>(ruta: string, cuerpo?: unknown) =>
    pedir<T>(ruta, { method: 'POST', body: cuerpo === undefined ? undefined : JSON.stringify(cuerpo) }),
  put: <T>(ruta: string, cuerpo?: unknown) =>
    pedir<T>(ruta, { method: 'PUT', body: cuerpo === undefined ? undefined : JSON.stringify(cuerpo) }),
  del: <T>(ruta: string) => pedir<T>(ruta, { method: 'DELETE' }),
}
