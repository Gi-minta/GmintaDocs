import { useContext } from 'react'
import { AuthContext } from './contexto'
import type { ContextoAuth } from './contexto'

export function useAuth(): ContextoAuth {
  const contexto = useContext(AuthContext)
  if (!contexto) throw new Error('useAuth debe usarse dentro de <ProveedorAuth>.')
  return contexto
}
