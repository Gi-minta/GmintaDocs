import { useState } from 'react'
import { api, ErrorApi } from '../api/cliente'
import { useLista } from '../hooks/useLista'
import { useListaPaginada } from '../hooks/useListaPaginada'
import { BotonEliminar } from '../componentes/BotonEliminar'
import { Paginacion } from '../componentes/Paginacion'
import type { Campo, Formulario } from '../tipos/modelos'

export function Formularios() {
  const { datos, cargando, error, recargar, setError, pagina, setPagina, total, totalPaginas } =
    useListaPaginada<Formulario>('/formularios')
  const [codigo, setCodigo] = useState('')
  const [tabla, setTabla] = useState('')
  const [nombre, setNombre] = useState('')
  const [seleccionado, setSeleccionado] = useState<number | null>(null)
  const [editId, setEditId] = useState<number | null>(null)
  const [editNombre, setEditNombre] = useState('')

  async function crear(evento: React.FormEvent) {
    evento.preventDefault()
    try {
      await api.post('/formularios', { codigo, tabla, nombre })
      setCodigo('')
      setTabla('')
      setNombre('')
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo crear el formulario.')
    }
  }

  function iniciarEdicion(f: Formulario) {
    setEditId(f.id)
    setEditNombre(f.nombre)
  }

  async function guardar(f: Formulario) {
    try {
      await api.put(`/formularios/${f.id}`, { nombre: editNombre, descripcion: f.descripcion })
      setEditId(null)
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo actualizar el formulario.')
    }
  }

  async function eliminar(id: number) {
    try {
      await api.del(`/formularios/${id}`)
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo eliminar el formulario.')
    }
  }

  return (
    <section>
      <h1>Formularios</h1>

      <form className="tarjeta formulario-en-linea" onSubmit={crear}>
        <input placeholder="Código" value={codigo} onChange={(e) => setCodigo(e.target.value)} required />
        <input placeholder="Tabla" value={tabla} onChange={(e) => setTabla(e.target.value)} required />
        <input placeholder="Nombre" value={nombre} onChange={(e) => setNombre(e.target.value)} required />
        <button type="submit">Crear</button>
      </form>

      {error && <p className="error">{error}</p>}
      {cargando ? (
        <p className="sutil">Cargando…</p>
      ) : (
        <table className="tabla">
          <thead>
            <tr>
              <th>#</th>
              <th>Código</th>
              <th>Nombre</th>
              <th>Tabla</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {datos.map((f) => (
              <tr key={f.id}>
                <td>{f.id}</td>
                <td>{f.codigo}</td>
                <td>
                  {editId === f.id ? (
                    <input value={editNombre} onChange={(e) => setEditNombre(e.target.value)} />
                  ) : (
                    f.nombre
                  )}
                </td>
                <td>{f.tabla}</td>
                <td>
                  <div className="acciones">
                    {editId === f.id ? (
                      <>
                        <button className="enlace" onClick={() => guardar(f)}>
                          Guardar
                        </button>
                        <button className="enlace" onClick={() => setEditId(null)}>
                          Cancelar
                        </button>
                      </>
                    ) : (
                      <>
                        <button className="enlace" onClick={() => setSeleccionado(seleccionado === f.id ? null : f.id)}>
                          {seleccionado === f.id ? 'Ocultar campos' : 'Campos'}
                        </button>
                        <button className="enlace" onClick={() => iniciarEdicion(f)}>
                          Editar
                        </button>
                        <BotonEliminar onEliminar={() => eliminar(f.id)} />
                      </>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      <Paginacion pagina={pagina} totalPaginas={totalPaginas} total={total} onCambiar={setPagina} />

      {seleccionado !== null && <Campos idFormulario={seleccionado} />}
    </section>
  )
}

function Campos({ idFormulario }: { idFormulario: number }) {
  const { datos, error, recargar, setError } = useLista<Campo>(`/formularios/${idFormulario}/campos`)
  const [orden, setOrden] = useState('1')
  const [nombre, setNombre] = useState('')
  const [columna, setColumna] = useState('')
  const [requerido, setRequerido] = useState(false)

  async function agregar(evento: React.FormEvent) {
    evento.preventDefault()
    try {
      await api.post(`/formularios/${idFormulario}/campos`, {
        orden: Number(orden),
        nombre,
        columna,
        tipoDato: 1,
        longDato: 0,
        control: 1,
        requerido,
      })
      setNombre('')
      setColumna('')
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo agregar el campo.')
    }
  }

  return (
    <div className="tarjeta" style={{ marginTop: '1rem' }}>
      <h3>Campos del formulario #{idFormulario}</h3>
      <form className="formulario-en-linea" onSubmit={agregar}>
        <input type="number" min={1} placeholder="Orden" value={orden} onChange={(e) => setOrden(e.target.value)} required />
        <input placeholder="Nombre" value={nombre} onChange={(e) => setNombre(e.target.value)} required />
        <input placeholder="Columna" value={columna} onChange={(e) => setColumna(e.target.value)} required />
        <label className="sutil" style={{ display: 'flex', alignItems: 'center', gap: '0.3rem' }}>
          <input
            type="checkbox"
            checked={requerido}
            onChange={(e) => setRequerido(e.target.checked)}
            style={{ width: 'auto' }}
          />
          Requerido
        </label>
        <button type="submit">Añadir</button>
      </form>
      {error && <p className="error">{error}</p>}
      {datos.length === 0 ? (
        <p className="sutil">Sin campos.</p>
      ) : (
        <ul>
          {datos.map((c) => (
            <li key={c.id}>
              {c.orden}. <strong>{c.nombre}</strong> → {c.columna} {c.requerido && '(requerido)'}
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
