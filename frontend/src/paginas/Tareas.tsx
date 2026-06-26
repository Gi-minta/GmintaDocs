import { useState } from 'react'
import { api, ErrorApi } from '../api/cliente'
import { BotonEliminar } from '../componentes/BotonEliminar'
import type { Tarea } from '../tipos/modelos'

const ESTADOS: Record<number, string> = {
  0: 'Pendiente',
  1: 'En curso',
  2: 'Finalizada',
}

export function Tareas() {
  const [responsable, setResponsable] = useState('')
  const [tareas, setTareas] = useState<Tarea[]>([])
  const [buscado, setBuscado] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const [editId, setEditId] = useState<number | null>(null)
  const [editAsunto, setEditAsunto] = useState('')
  const [editPrioridad, setEditPrioridad] = useState('')
  const [editResponsable, setEditResponsable] = useState('')
  const [editFecha, setEditFecha] = useState('')

  async function buscar(evento: React.FormEvent) {
    evento.preventDefault()
    setError(null)
    try {
      setTareas(await api.get<Tarea[]>(`/tareas/responsable/${encodeURIComponent(responsable)}`))
      setBuscado(true)
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudieron cargar las tareas.')
    }
  }

  async function ejecutar(id: number) {
    setError(null)
    try {
      await api.post(`/tareas/${id}/ejecutar`, { estado: 2 })
      await buscarSilencioso()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo ejecutar la tarea.')
    }
  }

  function iniciarEdicion(t: Tarea) {
    setEditId(t.id)
    setEditAsunto(t.asunto)
    setEditPrioridad(t.prioridad)
    setEditResponsable(t.responsable)
    setEditFecha(t.fechaVencimiento.slice(0, 10))
  }

  async function guardar(id: number) {
    setError(null)
    try {
      await api.put(`/tareas/${id}`, {
        asunto: editAsunto,
        descripcion: null,
        prioridad: editPrioridad,
        responsable: editResponsable,
        fechaVencimiento: new Date(editFecha).toISOString(),
      })
      setEditId(null)
      await buscarSilencioso()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo actualizar la tarea.')
    }
  }

  async function eliminar(id: number) {
    setError(null)
    try {
      await api.del(`/tareas/${id}`)
      await buscarSilencioso()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo eliminar la tarea.')
    }
  }

  async function buscarSilencioso() {
    if (!responsable) return
    setTareas(await api.get<Tarea[]>(`/tareas/responsable/${encodeURIComponent(responsable)}`))
  }

  return (
    <section>
      <h1>Tareas</h1>

      <form className="tarjeta formulario-en-linea" onSubmit={buscar}>
        <input
          placeholder="Responsable"
          value={responsable}
          onChange={(e) => setResponsable(e.target.value)}
          required
        />
        <button type="submit">Buscar</button>
      </form>

      {error && <p className="error">{error}</p>}

      {buscado &&
        (tareas.length === 0 ? (
          <p className="sutil">Sin tareas para «{responsable}».</p>
        ) : (
          <table className="tabla">
            <thead>
              <tr>
                <th>#</th>
                <th>Asunto</th>
                <th>Prioridad</th>
                <th>Estado</th>
                <th>Vence</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {tareas.map((t) => (
                <tr key={t.id}>
                  <td>{t.id}</td>
                  <td>
                    {editId === t.id ? (
                      <input value={editAsunto} onChange={(e) => setEditAsunto(e.target.value)} />
                    ) : (
                      t.asunto
                    )}
                  </td>
                  <td>
                    {editId === t.id ? (
                      <input value={editPrioridad} onChange={(e) => setEditPrioridad(e.target.value)} />
                    ) : (
                      t.prioridad
                    )}
                  </td>
                  <td>{ESTADOS[t.estado] ?? t.estado}</td>
                  <td>
                    {editId === t.id ? (
                      <input type="date" value={editFecha} onChange={(e) => setEditFecha(e.target.value)} />
                    ) : (
                      new Date(t.fechaVencimiento).toLocaleDateString()
                    )}
                  </td>
                  <td>
                    <div className="acciones">
                      {editId === t.id ? (
                        <>
                          <input
                            value={editResponsable}
                            onChange={(e) => setEditResponsable(e.target.value)}
                            placeholder="Responsable"
                          />
                          <button className="enlace" onClick={() => guardar(t.id)}>
                            Guardar
                          </button>
                          <button className="enlace" onClick={() => setEditId(null)}>
                            Cancelar
                          </button>
                        </>
                      ) : (
                        <>
                          {t.estado !== 2 && (
                            <button className="enlace" onClick={() => ejecutar(t.id)}>
                              Finalizar
                            </button>
                          )}
                          <button className="enlace" onClick={() => iniciarEdicion(t)}>
                            Editar
                          </button>
                          <BotonEliminar onEliminar={() => eliminar(t.id)} confirmacion="¿Eliminar esta tarea?" />
                        </>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        ))}
    </section>
  )
}
