import { useState } from 'react'
import { api, ErrorApi } from '../api/cliente'
import { useListaPaginada } from '../hooks/useListaPaginada'
import { Paginacion } from '../componentes/Paginacion'
import type { Rol } from '../tipos/modelos'

export function Roles() {
  const { datos, cargando, error, recargar, setError, pagina, setPagina, total, totalPaginas } =
    useListaPaginada<Rol>('/roles')
  const [nombre, setNombre] = useState('')

  async function crear(evento: React.FormEvent) {
    evento.preventDefault()
    try {
      await api.post('/roles', { nombre })
      setNombre('')
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo crear el rol.')
    }
  }

  return (
    <section>
      <h1>Roles</h1>

      <form className="tarjeta formulario-en-linea" onSubmit={crear}>
        <input placeholder="Nombre del rol" value={nombre} onChange={(e) => setNombre(e.target.value)} required />
        <button type="submit">Crear</button>
      </form>

      {error && <p className="error">{error}</p>}
      {cargando ? (
        <p className="sutil">Cargando…</p>
      ) : datos.length === 0 ? (
        <p className="sutil">Aún no hay roles.</p>
      ) : (
        <ul className="lista">
          {datos.map((r) => (
            <li key={r.id} className="tarjeta">
              {r.nombre}
            </li>
          ))}
        </ul>
      )}

      <Paginacion pagina={pagina} totalPaginas={totalPaginas} total={total} onCambiar={setPagina} />
    </section>
  )
}
