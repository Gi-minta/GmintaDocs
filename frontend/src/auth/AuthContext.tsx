import { useCallback, useEffect, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import { api, alPerderSesion, establecerToken } from '../api/cliente'
import type { RespuestaLogin, UsuarioAutenticado } from '../tipos/modelos'
import { AuthContext } from './contexto'
import type { ContextoAuth } from './contexto'

const CLAVE_ALMACEN = 'gmintadocs.sesion'

interface SesionGuardada {
  token: string
  usuario: UsuarioAutenticado
  idEmpresa: number
}

function leerSesion(): SesionGuardada | null {
  const crudo = localStorage.getItem(CLAVE_ALMACEN)
  if (!crudo) return null
  try {
    return JSON.parse(crudo) as SesionGuardada
  } catch {
    return null
  }
}

export function ProveedorAuth({ children }: { children: ReactNode }) {
  const [sesion, setSesion] = useState<SesionGuardada | null>(() => {
    const guardada = leerSesion()
    if (guardada) establecerToken(guardada.token)
    return guardada
  })

  const cerrarSesion = useCallback(() => {
    establecerToken(null)
    localStorage.removeItem(CLAVE_ALMACEN)
    setSesion(null)
  }, [])

  useEffect(() => {
    alPerderSesion(cerrarSesion)
  }, [cerrarSesion])

  const iniciarSesion = useCallback(
    async (idEmpresa: number, userName: string, contrasena: string) => {
      const respuesta = await api.post<RespuestaLogin>('/auth/login', {
        userName,
        contrasena,
        idEmpresa,
      })
      const nueva: SesionGuardada = { token: respuesta.token, usuario: respuesta.usuario, idEmpresa }
      establecerToken(respuesta.token)
      localStorage.setItem(CLAVE_ALMACEN, JSON.stringify(nueva))
      setSesion(nueva)
    },
    [],
  )

  const valor = useMemo<ContextoAuth>(
    () => ({
      usuario: sesion?.usuario ?? null,
      idEmpresa: sesion?.idEmpresa ?? null,
      autenticado: sesion !== null,
      iniciarSesion,
      cerrarSesion,
    }),
    [sesion, iniciarSesion, cerrarSesion],
  )

  return <AuthContext value={valor}>{children}</AuthContext>
}
