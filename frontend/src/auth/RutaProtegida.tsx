import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from './useAuth'

/** Permite el acceso a las rutas hijas sólo si hay una sesión activa. */
export function RutaProtegida() {
  const { autenticado } = useAuth()
  return autenticado ? <Outlet /> : <Navigate to="/login" replace />
}
