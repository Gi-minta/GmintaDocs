import { useState } from 'react'
import { api, ErrorApi } from '../api/cliente'
import { useLista } from '../hooks/useLista'
import { useListaPaginada } from '../hooks/useListaPaginada'
import { BotonEliminar } from '../componentes/BotonEliminar'
import { Paginacion } from '../componentes/Paginacion'
import type { Plantilla, PlantillaFormato } from '../tipos/modelos'

export function Plantillas() {
  return (
    <section>
      <h1>Plantillas</h1>
      <ListaPlantillas />
      <ListaFormatos />
    </section>
  )
}

function ListaPlantillas() {
  const { datos, cargando, error, recargar, setError, pagina, setPagina, total, totalPaginas } =
    useListaPaginada<Plantilla>('/plantillas')
  const [codigo, setCodigo] = useState('')
  const [nombre, setNombre] = useState('')
  const [contenido, setContenido] = useState('')
  const [editId, setEditId] = useState<number | null>(null)
  const [editNombre, setEditNombre] = useState('')
  const [editContenido, setEditContenido] = useState('')

  async function crear(evento: React.FormEvent) {
    evento.preventDefault()
    try {
      await api.post('/plantillas', { codigo, nombre, contenido })
      setCodigo('')
      setNombre('')
      setContenido('')
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo crear la plantilla.')
    }
  }

  function iniciarEdicion(p: Plantilla) {
    setEditId(p.id)
    setEditNombre(p.nombre)
    setEditContenido(p.contenido)
  }

  async function guardar(id: number) {
    try {
      await api.put(`/plantillas/${id}`, { nombre: editNombre, contenido: editContenido })
      setEditId(null)
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo actualizar la plantilla.')
    }
  }

  async function eliminar(id: number) {
    try {
      await api.del(`/plantillas/${id}`)
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo eliminar la plantilla.')
    }
  }

  return (
    <div className="tarjeta" style={{ marginBottom: '1.2rem' }}>
      <h3>Plantillas de contenido</h3>
      <form className="formulario-en-linea" onSubmit={crear}>
        <input placeholder="Código" value={codigo} onChange={(e) => setCodigo(e.target.value)} required />
        <input placeholder="Nombre" value={nombre} onChange={(e) => setNombre(e.target.value)} required />
        <input placeholder="Contenido" value={contenido} onChange={(e) => setContenido(e.target.value)} />
        <button type="submit">Crear</button>
      </form>
      {error && <p className="error">{error}</p>}
      {cargando ? (
        <p className="sutil">Cargando…</p>
      ) : datos.length === 0 ? (
        <p className="sutil">Sin plantillas.</p>
      ) : (
        <ul className="lista">
          {datos.map((p) => (
            <li key={p.id} className="fila">
              {editId === p.id ? (
                <span className="acciones" style={{ flex: 1 }}>
                  <input value={editNombre} onChange={(e) => setEditNombre(e.target.value)} placeholder="Nombre" />
                  <input value={editContenido} onChange={(e) => setEditContenido(e.target.value)} placeholder="Contenido" />
                  <button className="enlace" onClick={() => guardar(p.id)}>
                    Guardar
                  </button>
                  <button className="enlace" onClick={() => setEditId(null)}>
                    Cancelar
                  </button>
                </span>
              ) : (
                <>
                  <span>
                    <strong>{p.codigo}</strong> — {p.nombre}
                  </span>
                  <span className="acciones">
                    <button className="enlace" onClick={() => iniciarEdicion(p)}>
                      Editar
                    </button>
                    <BotonEliminar onEliminar={() => eliminar(p.id)} />
                  </span>
                </>
              )}
            </li>
          ))}
        </ul>
      )}
      <Paginacion pagina={pagina} totalPaginas={totalPaginas} total={total} onCambiar={setPagina} />
    </div>
  )
}

function ListaFormatos() {
  const { datos, cargando, error, recargar, setError } = useLista<PlantillaFormato>('/plantillas-formato')
  const [codigo, setCodigo] = useState('')
  const [nombre, setNombre] = useState('')
  const [idFormulario, setIdFormulario] = useState('')

  async function crear(evento: React.FormEvent) {
    evento.preventDefault()
    try {
      await api.post('/plantillas-formato', {
        codigo,
        nombre,
        formatoHtml: null,
        idFormulario: Number(idFormulario),
      })
      setCodigo('')
      setNombre('')
      setIdFormulario('')
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo crear el formato.')
    }
  }

  return (
    <div className="tarjeta">
      <h3>Formatos por formulario</h3>
      <form className="formulario-en-linea" onSubmit={crear}>
        <input placeholder="Código" value={codigo} onChange={(e) => setCodigo(e.target.value)} required />
        <input placeholder="Nombre" value={nombre} onChange={(e) => setNombre(e.target.value)} required />
        <input type="number" min={1} placeholder="Id formulario" value={idFormulario} onChange={(e) => setIdFormulario(e.target.value)} required />
        <button type="submit">Crear</button>
      </form>
      {error && <p className="error">{error}</p>}
      {cargando ? (
        <p className="sutil">Cargando…</p>
      ) : datos.length === 0 ? (
        <p className="sutil">Sin formatos.</p>
      ) : (
        <ul>
          {datos.map((f) => (
            <li key={f.id}>
              <strong>{f.codigo}</strong> — {f.nombre} <span className="sutil">(formulario {f.idFormulario})</span>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
