import { useState } from 'react'
import { api, ErrorApi } from '../api/cliente'
import { useListaPaginada } from '../hooks/useListaPaginada'
import { BotonEliminar } from '../componentes/BotonEliminar'
import { Paginacion } from '../componentes/Paginacion'
import type { Archivo, TipoDocumento } from '../tipos/modelos'

export function Documentos() {
  return (
    <section>
      <h1>Gestión documental</h1>
      <TiposDocumento />
      <Archivos />
    </section>
  )
}

function TiposDocumento() {
  const { datos, cargando, error, recargar, setError, pagina, setPagina, total, totalPaginas } =
    useListaPaginada<TipoDocumento>('/tipos-documento')
  const [codigo, setCodigo] = useState('')
  const [nombre, setNombre] = useState('')
  const [controlaVersion, setControlaVersion] = useState(false)
  const [editId, setEditId] = useState<number | null>(null)
  const [editNombre, setEditNombre] = useState('')
  const [editControlaVersion, setEditControlaVersion] = useState(false)

  async function crear(evento: React.FormEvent) {
    evento.preventDefault()
    try {
      await api.post('/tipos-documento', {
        codigo,
        nombre,
        controlaVersion,
        diasVigencia: 0,
        controlaVigencia: false,
      })
      setCodigo('')
      setNombre('')
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo crear el tipo de documento.')
    }
  }

  function iniciarEdicion(t: TipoDocumento) {
    setEditId(t.id)
    setEditNombre(t.nombre)
    setEditControlaVersion(t.controlaVersion)
  }

  async function guardar(t: TipoDocumento) {
    try {
      await api.put(`/tipos-documento/${t.id}`, {
        nombre: editNombre,
        controlaVersion: editControlaVersion,
        diasVigencia: t.diasVigencia,
        controlaVigencia: t.controlaVigencia,
      })
      setEditId(null)
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo actualizar el tipo de documento.')
    }
  }

  async function eliminar(id: number) {
    try {
      await api.del(`/tipos-documento/${id}`)
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo eliminar el tipo de documento.')
    }
  }

  return (
    <div className="tarjeta" style={{ marginBottom: '1.2rem' }}>
      <h3>Tipos de documento</h3>
      <form className="formulario-en-linea" onSubmit={crear}>
        <input placeholder="Código" value={codigo} onChange={(e) => setCodigo(e.target.value)} required />
        <input placeholder="Nombre" value={nombre} onChange={(e) => setNombre(e.target.value)} required />
        <label className="sutil" style={{ display: 'flex', alignItems: 'center', gap: '0.3rem' }}>
          <input type="checkbox" checked={controlaVersion} onChange={(e) => setControlaVersion(e.target.checked)} style={{ width: 'auto' }} />
          Controla versión
        </label>
        <button type="submit">Crear</button>
      </form>
      {error && <p className="error">{error}</p>}
      {cargando ? (
        <p className="sutil">Cargando…</p>
      ) : datos.length === 0 ? (
        <p className="sutil">Sin tipos de documento.</p>
      ) : (
        <ul className="lista">
          {datos.map((t) => (
            <li key={t.id} className="fila">
              {editId === t.id ? (
                <span className="acciones" style={{ flex: 1 }}>
                  <input value={editNombre} onChange={(e) => setEditNombre(e.target.value)} placeholder="Nombre" />
                  <label className="sutil" style={{ display: 'flex', alignItems: 'center', gap: '0.3rem' }}>
                    <input
                      type="checkbox"
                      checked={editControlaVersion}
                      onChange={(e) => setEditControlaVersion(e.target.checked)}
                      style={{ width: 'auto' }}
                    />
                    Versionado
                  </label>
                  <button className="enlace" onClick={() => guardar(t)}>
                    Guardar
                  </button>
                  <button className="enlace" onClick={() => setEditId(null)}>
                    Cancelar
                  </button>
                </span>
              ) : (
                <>
                  <span>
                    <strong>{t.codigo}</strong> — {t.nombre} {t.controlaVersion && <span className="sutil">(versionado)</span>}
                  </span>
                  <span className="acciones">
                    <button className="enlace" onClick={() => iniciarEdicion(t)}>
                      Editar
                    </button>
                    <BotonEliminar onEliminar={() => eliminar(t.id)} confirmacion="¿Eliminar este tipo de documento?" />
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

function Archivos() {
  const [directorio, setDirectorio] = useState('')
  const [archivos, setArchivos] = useState<Archivo[]>([])
  const [buscado, setBuscado] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const [nombre, setNombre] = useState('')
  const [extension, setExtension] = useState('')
  const [idTipoDocumento, setIdTipoDocumento] = useState('')

  async function cargar() {
    setArchivos(await api.get<Archivo[]>(`/archivos/directorio/${encodeURIComponent(directorio)}`))
    setBuscado(true)
  }

  async function buscar(evento: React.FormEvent) {
    evento.preventDefault()
    setError(null)
    try {
      await cargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudieron cargar los archivos.')
    }
  }

  async function registrar(evento: React.FormEvent) {
    evento.preventDefault()
    setError(null)
    try {
      await api.post('/archivos', {
        nombre,
        extension,
        directorio,
        idTipoDocumento: Number(idTipoDocumento),
        bytes: 0,
        version: null,
        descripcion: null,
      })
      setNombre('')
      setExtension('')
      await cargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo registrar el archivo.')
    }
  }

  async function eliminar(id: number) {
    setError(null)
    try {
      await api.del(`/archivos/${id}`)
      await cargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo eliminar el archivo.')
    }
  }

  return (
    <div className="tarjeta">
      <h3>Archivos por directorio</h3>
      <form className="formulario-en-linea" onSubmit={buscar}>
        <input placeholder="Directorio" value={directorio} onChange={(e) => setDirectorio(e.target.value)} required />
        <button type="submit">Cargar</button>
      </form>

      {buscado && (
        <form className="formulario-en-linea" onSubmit={registrar}>
          <input placeholder="Nombre" value={nombre} onChange={(e) => setNombre(e.target.value)} required />
          <input placeholder="Extensión" value={extension} onChange={(e) => setExtension(e.target.value)} required />
          <input type="number" min={1} placeholder="Id tipo doc." value={idTipoDocumento} onChange={(e) => setIdTipoDocumento(e.target.value)} required />
          <button type="submit">Registrar archivo</button>
        </form>
      )}

      {error && <p className="error">{error}</p>}

      {buscado &&
        (archivos.length === 0 ? (
          <p className="sutil">Sin archivos en «{directorio}».</p>
        ) : (
          <ul className="lista">
            {archivos.map((a) => (
              <li key={a.id} className="fila">
                <span>
                  <strong>{a.nombre}.{a.extension}</strong> <span className="sutil">v{a.version}</span>
                </span>
                <BotonEliminar onEliminar={() => eliminar(a.id)} confirmacion="¿Eliminar este archivo?" />
              </li>
            ))}
          </ul>
        ))}
    </div>
  )
}
