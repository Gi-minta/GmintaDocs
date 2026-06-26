import { createContext } from 'react'
import type { UsuarioAutenticado } from '../tipos/modelos'

export interface ContextoAuth {
  usuario: UsuarioAutenticado | null
  idEmpresa: number | null
  autenticado: boolean
  iniciarSesion: (idEmpresa: number, userName: string, contrasena: string) => Promise<void>
  cerrarSesion: () => void
}

export const AuthContext = createContext<ContextoAuth | null>(null)
