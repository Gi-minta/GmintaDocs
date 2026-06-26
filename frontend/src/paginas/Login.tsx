import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import { ErrorApi } from '../api/cliente'

export function Login() {
  const { iniciarSesion } = useAuth()
  const navegar = useNavigate()

  const [idEmpresa, setIdEmpresa] = useState('1')
  const [userName, setUserName] = useState('')
  const [contrasena, setContrasena] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [enviando, setEnviando] = useState(false)

  async function enviar(evento: React.FormEvent) {
    evento.preventDefault()
    setError(null)
    setEnviando(true)
    try {
      await iniciarSesion(Number(idEmpresa), userName, contrasena)
      navegar('/', { replace: true })
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo iniciar sesión.')
    } finally {
      setEnviando(false)
    }
  }

  return (
    <div className="login">
      <form className="tarjeta login-tarjeta" onSubmit={enviar}>
        <h1>GmintaDocs</h1>
        <p className="sutil">Gestión documental multiempresa</p>

        <label>
          Empresa
          <input
            type="number"
            min={1}
            value={idEmpresa}
            onChange={(e) => setIdEmpresa(e.target.value)}
            required
          />
        </label>
        <label>
          Usuario
          <input value={userName} onChange={(e) => setUserName(e.target.value)} required autoFocus />
        </label>
        <label>
          Contraseña
          <input
            type="password"
            value={contrasena}
            onChange={(e) => setContrasena(e.target.value)}
            required
          />
        </label>

        {error && <p className="error">{error}</p>}

        <button type="submit" disabled={enviando}>
          {enviando ? 'Entrando…' : 'Entrar'}
        </button>
      </form>
    </div>
  )
}
