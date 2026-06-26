import { useState } from 'react'
import { api, ErrorApi } from '../api/cliente'
import { useLista } from '../hooks/useLista'
import { useListaPaginada } from '../hooks/useListaPaginada'
import { BotonEliminar } from '../componentes/BotonEliminar'
import { Paginacion } from '../componentes/Paginacion'
import type { CategoriaReporte, Reporte } from '../tipos/modelos'

export function Reportes() {
  const { datos, cargando, error, recargar, setError, pagina, setPagina, total, totalPaginas } =
    useListaPaginada<CategoriaReporte>('/categorias-reporte')
  const [codigo, setCodigo] = useState('')
  const [categoria, setCategoria] = useState('')
  const [seleccionada, setSeleccionada] = useState<number | null>(null)

  async function crear(evento: React.FormEvent) {
    evento.preventDefault()
    try {
      await api.post('/categorias-reporte', { codigo, categoria, descripcion: null })
      setCodigo('')
      setCategoria('')
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo crear la categoría.')
    }
  }

  return (
    <section>
      <h1>Reportes</h1>

      <form className="tarjeta formulario-en-linea" onSubmit={crear}>
        <input placeholder="Código" value={codigo} onChange={(e) => setCodigo(e.target.value)} required />
        <input placeholder="Categoría" value={categoria} onChange={(e) => setCategoria(e.target.value)} required />
        <button type="submit">Crear categoría</button>
      </form>

      {error && <p className="error">{error}</p>}
      {cargando ? (
        <p className="sutil">Cargando…</p>
      ) : datos.length === 0 ? (
        <p className="sutil">Sin categorías.</p>
      ) : (
        <ul className="lista">
          {datos.map((c) => (
            <li key={c.id} className="tarjeta">
              <div className="fila">
                <span>
                  <strong>{c.codigo}</strong> — {c.categoria}
                </span>
                <button className="enlace" onClick={() => setSeleccionada(seleccionada === c.id ? null : c.id)}>
                  {seleccionada === c.id ? 'Ocultar reportes' : 'Reportes'}
                </button>
              </div>
              {seleccionada === c.id && <ReportesDeCategoria idCategoria={c.id} />}
            </li>
          ))}
        </ul>
      )}

      <Paginacion pagina={pagina} totalPaginas={totalPaginas} total={total} onCambiar={setPagina} />
    </section>
  )
}

function ReportesDeCategoria({ idCategoria }: { idCategoria: number }) {
  const { datos, error, recargar, setError } = useLista<Reporte>(`/categorias-reporte/${idCategoria}/reportes`)
  const [codigo, setCodigo] = useState('')
  const [reporte, setReporte] = useState('')
  const [url, setUrl] = useState('')
  const [editId, setEditId] = useState<number | null>(null)
  const [editReporte, setEditReporte] = useState('')
  const [editUrl, setEditUrl] = useState('')

  async function crear(evento: React.FormEvent) {
    evento.preventDefault()
    try {
      await api.post('/reportes', { idCategoria, codigo, reporte, url, descripcion: null })
      setCodigo('')
      setReporte('')
      setUrl('')
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo crear el reporte.')
    }
  }

  function iniciarEdicion(r: Reporte) {
    setEditId(r.id)
    setEditReporte(r.reporte)
    setEditUrl(r.url)
  }

  async function guardar(id: number) {
    try {
      await api.put(`/reportes/${id}`, { reporte: editReporte, url: editUrl, descripcion: null })
      setEditId(null)
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo actualizar el reporte.')
    }
  }

  async function eliminar(id: number) {
    try {
      await api.del(`/reportes/${id}`)
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo eliminar el reporte.')
    }
  }

  return (
    <div className="comentarios">
      {datos.length === 0 ? (
        <p className="sutil">Sin reportes en esta categoría.</p>
      ) : (
        datos.map((r) => (
          <div key={r.id} className="fila">
            {editId === r.id ? (
              <span className="acciones" style={{ flex: 1 }}>
                <input value={editReporte} onChange={(e) => setEditReporte(e.target.value)} placeholder="Reporte" />
                <input value={editUrl} onChange={(e) => setEditUrl(e.target.value)} placeholder="URL" />
                <button className="enlace" onClick={() => guardar(r.id)}>
                  Guardar
                </button>
                <button className="enlace" onClick={() => setEditId(null)}>
                  Cancelar
                </button>
              </span>
            ) : (
              <>
                <span>
                  <strong>{r.codigo}</strong> — {r.reporte} <span className="sutil">({r.url})</span>
                </span>
                <span className="acciones">
                  <button className="enlace" onClick={() => iniciarEdicion(r)}>
                    Editar
                  </button>
                  <BotonEliminar onEliminar={() => eliminar(r.id)} confirmacion="¿Eliminar este reporte?" />
                </span>
              </>
            )}
          </div>
        ))
      )}
      <form className="formulario-en-linea" onSubmit={crear}>
        <input placeholder="Código" value={codigo} onChange={(e) => setCodigo(e.target.value)} required />
        <input placeholder="Reporte" value={reporte} onChange={(e) => setReporte(e.target.value)} required />
        <input placeholder="URL" value={url} onChange={(e) => setUrl(e.target.value)} required />
        <button type="submit">Añadir</button>
      </form>
      {error && <p className="error">{error}</p>}
    </div>
  )
}
