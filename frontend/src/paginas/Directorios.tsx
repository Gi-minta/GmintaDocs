import { useState } from 'react'
import { api, ErrorApi } from '../api/cliente'
import { BotonEliminar } from '../componentes/BotonEliminar'
import type { Directorio } from '../tipos/modelos'

export function Directorios() {
  const [idFormulario, setIdFormulario] = useState('')
  const [directorios, setDirectorios] = useState<Directorio[]>([])
  const [buscado, setBuscado] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const [codigo, setCodigo] = useState('')
  const [nombre, setNombre] = useState('')
  const [idPadre, setIdPadre] = useState('0')

  const [editId, setEditId] = useState<number | null>(null)
  const [editNombre, setEditNombre] = useState('')
  const [editCodigo, setEditCodigo] = useState('')

  async function cargar() {
    setDirectorios(await api.get<Directorio[]>(`/directorios/formulario/${Number(idFormulario)}`))
    setBuscado(true)
  }

  function iniciarEdicion(d: Directorio) {
    setEditId(d.id)
    setEditNombre(d.nombre)
    setEditCodigo(d.codigo)
  }

  async function guardar(id: number) {
    setError(null)
    try {
      await api.put(`/directorios/${id}`, { nombre: editNombre, codigo: editCodigo })
      setEditId(null)
      await cargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo actualizar el directorio.')
    }
  }

  async function eliminar(id: number) {
    setError(null)
    try {
      await api.del(`/directorios/${id}`)
      await cargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo eliminar el directorio.')
    }
  }

  async function buscar(evento: React.FormEvent) {
    evento.preventDefault()
    setError(null)
    try {
      await cargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudieron cargar los directorios.')
    }
  }

  async function crear(evento: React.FormEvent) {
    evento.preventDefault()
    setError(null)
    try {
      await api.post('/directorios', {
        idFormulario: Number(idFormulario),
        idDirectorioPadre: Number(idPadre),
        codigo,
        nombre,
      })
      setCodigo('')
      setNombre('')
      await cargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo crear el directorio.')
    }
  }

  return (
    <section>
      <h1>Directorios</h1>

      <form className="tarjeta formulario-en-linea" onSubmit={buscar}>
        <input
          type="number"
          min={1}
          placeholder="Id de formulario"
          value={idFormulario}
          onChange={(e) => setIdFormulario(e.target.value)}
          required
        />
        <button type="submit">Cargar</button>
      </form>

      {buscado && (
        <form className="tarjeta formulario-en-linea" onSubmit={crear}>
          <input type="number" min={0} placeholder="Id padre" value={idPadre} onChange={(e) => setIdPadre(e.target.value)} required />
          <input placeholder="Código" value={codigo} onChange={(e) => setCodigo(e.target.value)} required />
          <input placeholder="Nombre" value={nombre} onChange={(e) => setNombre(e.target.value)} required />
          <button type="submit">Crear directorio</button>
        </form>
      )}

      {error && <p className="error">{error}</p>}

      {buscado &&
        (directorios.length === 0 ? (
          <p className="sutil">Sin directorios para el formulario {idFormulario}.</p>
        ) : (
          <ul className="lista">
            {directorios.map((d) => (
              <li key={d.id} className="tarjeta fila">
                {editId === d.id ? (
                  <span className="acciones" style={{ flex: 1 }}>
                    <input value={editCodigo} onChange={(e) => setEditCodigo(e.target.value)} placeholder="Código" />
                    <input value={editNombre} onChange={(e) => setEditNombre(e.target.value)} placeholder="Nombre" />
                    <button className="enlace" onClick={() => guardar(d.id)}>
                      Guardar
                    </button>
                    <button className="enlace" onClick={() => setEditId(null)}>
                      Cancelar
                    </button>
                  </span>
                ) : (
                  <>
                    <span>
                      <strong>{d.codigo}</strong> — {d.nombre}
                      <span className="sutil"> (padre: {d.idDirectorioPadre})</span>
                    </span>
                    <span className="acciones">
                      <button className="enlace" onClick={() => iniciarEdicion(d)}>
                        Editar
                      </button>
                      <BotonEliminar onEliminar={() => eliminar(d.id)} confirmacion="¿Eliminar este directorio?" />
                    </span>
                  </>
                )}
              </li>
            ))}
          </ul>
        ))}
    </section>
  )
}
