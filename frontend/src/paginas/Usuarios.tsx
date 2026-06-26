import { useState } from 'react'
import { api, ErrorApi } from '../api/cliente'
import { useListaPaginada } from '../hooks/useListaPaginada'
import { Paginacion } from '../componentes/Paginacion'
import type { Rol, Usuario } from '../tipos/modelos'

export function Usuarios() {
  const { datos: usuarios, cargando, error, recargar, setError, pagina, setPagina, total, totalPaginas } =
    useListaPaginada<Usuario>('/usuarios')
  const { datos: roles } = useListaPaginada<Rol>('/roles', 100)

  const [userName, setUserName] = useState('')
  const [fullName, setFullName] = useState('')
  const [email, setEmail] = useState('')
  const [contrasena, setContrasena] = useState('')

  async function crear(evento: React.FormEvent) {
    evento.preventDefault()
    try {
      await api.post('/usuarios', { userName, fullName, email, contrasena })
      setUserName('')
      setFullName('')
      setEmail('')
      setContrasena('')
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo crear el usuario.')
    }
  }

  async function asignarRol(idUsuario: string, idRol: string) {
    try {
      await api.post(`/usuarios/${idUsuario}/roles`, { idRol })
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo asignar el rol.')
    }
  }

  async function desactivar(idUsuario: string) {
    try {
      await api.post(`/usuarios/${idUsuario}/desactivar`)
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo desactivar el usuario.')
    }
  }

  return (
    <section>
      <h1>Usuarios</h1>

      <form className="tarjeta formulario-en-linea" onSubmit={crear}>
        <input placeholder="Usuario" value={userName} onChange={(e) => setUserName(e.target.value)} required />
        <input placeholder="Nombre" value={fullName} onChange={(e) => setFullName(e.target.value)} />
        <input placeholder="Email" type="email" value={email} onChange={(e) => setEmail(e.target.value)} />
        <input
          placeholder="Contraseña"
          type="password"
          value={contrasena}
          onChange={(e) => setContrasena(e.target.value)}
          required
        />
        <button type="submit">Crear</button>
      </form>

      {error && <p className="error">{error}</p>}
      {cargando ? (
        <p className="sutil">Cargando…</p>
      ) : (
        <table className="tabla">
          <thead>
            <tr>
              <th>Usuario</th>
              <th>Nombre</th>
              <th>Email</th>
              <th>Activo</th>
              <th>Roles</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            {usuarios.map((u) => (
              <tr key={u.id}>
                <td>{u.userName}</td>
                <td>{u.fullName}</td>
                <td>{u.email}</td>
                <td>{u.activo ? 'Sí' : 'No'}</td>
                <td>{u.roles.join(', ') || <span className="sutil">—</span>}</td>
                <td>
                  <div className="acciones">
                    <AsignarRol roles={roles} onAsignar={(idRol) => asignarRol(u.id, idRol)} />
                    {u.activo && (
                      <button className="enlace peligro" onClick={() => desactivar(u.id)}>
                        Desactivar
                      </button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      <Paginacion pagina={pagina} totalPaginas={totalPaginas} total={total} onCambiar={setPagina} />
    </section>
  )
}

/** Selector de rol por fila: elige un rol y lo asigna al usuario. */
function AsignarRol({ roles, onAsignar }: { roles: Rol[]; onAsignar: (idRol: string) => void | Promise<void> }) {
  const [idRol, setIdRol] = useState('')

  async function asignar() {
    if (!idRol) return
    await onAsignar(idRol)
    setIdRol('')
  }

  return (
    <span className="acciones">
      <select value={idRol} onChange={(e) => setIdRol(e.target.value)} style={{ width: 'auto' }}>
        <option value="">Rol…</option>
        {roles.map((r) => (
          <option key={r.id} value={r.id}>
            {r.nombre}
          </option>
        ))}
      </select>
      <button className="enlace" onClick={asignar} disabled={!idRol}>
        Asignar
      </button>
    </span>
  )
}
