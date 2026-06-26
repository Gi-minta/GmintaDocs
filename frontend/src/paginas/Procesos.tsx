import { useState } from 'react'
import { api, ErrorApi } from '../api/cliente'
import { useLista } from '../hooks/useLista'
import { useListaPaginada } from '../hooks/useListaPaginada'
import { BotonEliminar } from '../componentes/BotonEliminar'
import { Paginacion } from '../componentes/Paginacion'
import type { Paso, Proceso } from '../tipos/modelos'

export function Procesos() {
  const { datos, cargando, error, recargar, setError, pagina, setPagina, total, totalPaginas } =
    useListaPaginada<Proceso>('/procesos')
  const [nombre, setNombre] = useState('')
  const [descripcion, setDescripcion] = useState('')
  const [idFormulario, setIdFormulario] = useState('')
  const [seleccionado, setSeleccionado] = useState<number | null>(null)
  const [editId, setEditId] = useState<number | null>(null)
  const [editNombre, setEditNombre] = useState('')
  const [editDescripcion, setEditDescripcion] = useState('')

  async function crear(evento: React.FormEvent) {
    evento.preventDefault()
    try {
      await api.post('/procesos', {
        nombre,
        descripcion,
        idFormulario: Number(idFormulario),
        version: '1',
      })
      setNombre('')
      setDescripcion('')
      setIdFormulario('')
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo crear el proceso.')
    }
  }

  function iniciarEdicion(p: Proceso) {
    setEditId(p.id)
    setEditNombre(p.nombre)
    setEditDescripcion(p.descripcion)
  }

  async function guardar(id: number) {
    try {
      await api.put(`/procesos/${id}`, { nombre: editNombre, descripcion: editDescripcion })
      setEditId(null)
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo actualizar el proceso.')
    }
  }

  async function eliminar(id: number) {
    try {
      await api.del(`/procesos/${id}`)
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo eliminar el proceso.')
    }
  }

  return (
    <section>
      <h1>Procesos</h1>

      <form className="tarjeta formulario-en-linea" onSubmit={crear}>
        <input placeholder="Nombre" value={nombre} onChange={(e) => setNombre(e.target.value)} required />
        <input placeholder="Descripción" value={descripcion} onChange={(e) => setDescripcion(e.target.value)} />
        <input
          type="number"
          min={1}
          placeholder="Id formulario"
          value={idFormulario}
          onChange={(e) => setIdFormulario(e.target.value)}
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
              <th>#</th>
              <th>Nombre</th>
              <th>Versión</th>
              <th>Formulario</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {datos.map((p) => (
              <tr key={p.id}>
                <td>{p.id}</td>
                <td>
                  {editId === p.id ? (
                    <input value={editNombre} onChange={(e) => setEditNombre(e.target.value)} />
                  ) : (
                    p.nombre
                  )}
                </td>
                <td>{p.version}</td>
                <td>{p.idFormulario}</td>
                <td>
                  <div className="acciones">
                    {editId === p.id ? (
                      <>
                        <input
                          value={editDescripcion}
                          onChange={(e) => setEditDescripcion(e.target.value)}
                          placeholder="Descripción"
                        />
                        <button className="enlace" onClick={() => guardar(p.id)}>
                          Guardar
                        </button>
                        <button className="enlace" onClick={() => setEditId(null)}>
                          Cancelar
                        </button>
                      </>
                    ) : (
                      <>
                        <button className="enlace" onClick={() => setSeleccionado(seleccionado === p.id ? null : p.id)}>
                          {seleccionado === p.id ? 'Ocultar' : 'Pasos / Workflow'}
                        </button>
                        <button className="enlace" onClick={() => iniciarEdicion(p)}>
                          Editar
                        </button>
                        <BotonEliminar onEliminar={() => eliminar(p.id)} confirmacion="¿Eliminar este proceso?" />
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

      {seleccionado !== null && <DetalleProceso idProceso={seleccionado} />}
    </section>
  )
}

function DetalleProceso({ idProceso }: { idProceso: number }) {
  const { datos, error, recargar, setError } = useLista<Paso>(`/procesos/${idProceso}/pasos`)
  const [numero, setNumero] = useState('1')
  const [descripcion, setDescripcion] = useState('')
  const [plazo, setPlazo] = useState('1')

  const [idFormulario, setIdFormulario] = useState('')
  const [idRegistro, setIdRegistro] = useState('')
  const [aviso, setAviso] = useState<string | null>(null)

  async function agregarPaso(evento: React.FormEvent) {
    evento.preventDefault()
    setAviso(null)
    try {
      await api.post(`/procesos/${idProceso}/pasos`, {
        numero: Number(numero),
        descripcion,
        prioridad: 'Normal',
        plazo: Number(plazo),
        unidadPlazo: 'Dias',
      })
      setDescripcion('')
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo agregar el paso.')
    }
  }

  async function iniciarWorkflow(evento: React.FormEvent) {
    evento.preventDefault()
    setAviso(null)
    try {
      const r = await api.post<{ id: number }>(`/procesos/${idProceso}/workflows`, {
        idFormulario: Number(idFormulario),
        idRegistro: Number(idRegistro),
      })
      setAviso(`Workflow #${r.id} iniciado.`)
      setIdFormulario('')
      setIdRegistro('')
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo iniciar el workflow.')
    }
  }

  return (
    <div className="tarjeta" style={{ marginTop: '1rem' }}>
      <h3>Proceso #{idProceso}</h3>

      <form className="formulario-en-linea" onSubmit={agregarPaso}>
        <input type="number" min={1} placeholder="Nº" value={numero} onChange={(e) => setNumero(e.target.value)} required />
        <input placeholder="Descripción del paso" value={descripcion} onChange={(e) => setDescripcion(e.target.value)} required />
        <input type="number" min={0} placeholder="Plazo (días)" value={plazo} onChange={(e) => setPlazo(e.target.value)} required />
        <button type="submit">Añadir paso</button>
      </form>

      {datos.length === 0 ? (
        <p className="sutil">Sin pasos.</p>
      ) : (
        <ul>
          {datos.map((p) => (
            <li key={p.id}>
              {p.numero}. {p.descripcion} <span className="sutil">({p.plazo} {p.unidadPlazo})</span>
            </li>
          ))}
        </ul>
      )}

      <h3>Iniciar workflow</h3>
      <form className="formulario-en-linea" onSubmit={iniciarWorkflow}>
        <input type="number" min={1} placeholder="Id formulario" value={idFormulario} onChange={(e) => setIdFormulario(e.target.value)} required />
        <input type="number" min={1} placeholder="Id registro" value={idRegistro} onChange={(e) => setIdRegistro(e.target.value)} required />
        <button type="submit">Iniciar</button>
      </form>
      {aviso && <p className="sutil">{aviso}</p>}
      {error && <p className="error">{error}</p>}
    </div>
  )
}
