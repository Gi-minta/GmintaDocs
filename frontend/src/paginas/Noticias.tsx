import { useEffect, useState } from 'react'
import { api, ErrorApi } from '../api/cliente'
import { useListaPaginada } from '../hooks/useListaPaginada'
import { BotonEliminar } from '../componentes/BotonEliminar'
import { Paginacion } from '../componentes/Paginacion'
import type { Comentario, Noticia } from '../tipos/modelos'

export function Noticias() {
  const { datos: noticias, cargando, error, recargar, setError, pagina, setPagina, total, totalPaginas } =
    useListaPaginada<Noticia>('/noticias')

  const [titulo, setTitulo] = useState('')
  const [autor, setAutor] = useState('')
  const [texto, setTexto] = useState('')

  const [seleccionada, setSeleccionada] = useState<number | null>(null)
  const [editId, setEditId] = useState<number | null>(null)
  const [editTitulo, setEditTitulo] = useState('')
  const [editTexto, setEditTexto] = useState('')

  function iniciarEdicion(n: Noticia) {
    setEditId(n.id)
    setEditTitulo(n.titulo)
    setEditTexto(n.texto)
  }

  async function guardar(id: number) {
    setError(null)
    try {
      await api.put(`/noticias/${id}`, { titulo: editTitulo, texto: editTexto })
      setEditId(null)
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo actualizar la noticia.')
    }
  }

  async function eliminar(id: number) {
    setError(null)
    try {
      await api.del(`/noticias/${id}`)
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo eliminar la noticia.')
    }
  }

  async function publicar(evento: React.FormEvent) {
    evento.preventDefault()
    setError(null)
    try {
      await api.post('/noticias', { titulo, autor, avatar: null, texto })
      setTitulo('')
      setAutor('')
      setTexto('')
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo publicar la noticia.')
    }
  }

  return (
    <section>
      <h1>Noticias</h1>

      <form className="tarjeta formulario-en-linea" onSubmit={publicar}>
        <input placeholder="Título" value={titulo} onChange={(e) => setTitulo(e.target.value)} required />
        <input placeholder="Autor" value={autor} onChange={(e) => setAutor(e.target.value)} required />
        <input placeholder="Texto" value={texto} onChange={(e) => setTexto(e.target.value)} required />
        <button type="submit">Publicar</button>
      </form>

      {error && <p className="error">{error}</p>}
      {cargando ? (
        <p className="sutil">Cargando…</p>
      ) : noticias.length === 0 ? (
        <p className="sutil">Aún no hay noticias.</p>
      ) : (
        <ul className="lista">
          {noticias.map((n) => (
            <li key={n.id} className="tarjeta">
              {editId === n.id ? (
                <div className="acciones" style={{ flexDirection: 'column', alignItems: 'stretch' }}>
                  <input value={editTitulo} onChange={(e) => setEditTitulo(e.target.value)} placeholder="Título" />
                  <textarea value={editTexto} onChange={(e) => setEditTexto(e.target.value)} placeholder="Texto" rows={3} />
                  <span className="acciones">
                    <button className="enlace" onClick={() => guardar(n.id)}>
                      Guardar
                    </button>
                    <button className="enlace" onClick={() => setEditId(null)}>
                      Cancelar
                    </button>
                  </span>
                </div>
              ) : (
                <>
                  <div className="fila">
                    <strong>{n.titulo}</strong>
                    <span className="sutil">{new Date(n.fechaPublicacion).toLocaleString()}</span>
                  </div>
                  <p>{n.texto}</p>
                  <div className="fila">
                    <span className="sutil">por {n.autor}</span>
                    <span className="acciones">
                      <button className="enlace" onClick={() => setSeleccionada(seleccionada === n.id ? null : n.id)}>
                        {seleccionada === n.id ? 'Ocultar comentarios' : 'Comentarios'}
                      </button>
                      <button className="enlace" onClick={() => iniciarEdicion(n)}>
                        Editar
                      </button>
                      <BotonEliminar onEliminar={() => eliminar(n.id)} confirmacion="¿Eliminar esta noticia?" />
                    </span>
                  </div>
                  {seleccionada === n.id && <Comentarios idNoticia={n.id} />}
                </>
              )}
            </li>
          ))}
        </ul>
      )}

      <Paginacion pagina={pagina} totalPaginas={totalPaginas} total={total} onCambiar={setPagina} />
    </section>
  )
}

function Comentarios({ idNoticia }: { idNoticia: number }) {
  const [comentarios, setComentarios] = useState<Comentario[]>([])
  const [autor, setAutor] = useState('')
  const [texto, setTexto] = useState('')
  const [error, setError] = useState<string | null>(null)

  async function recargar() {
    try {
      setComentarios(await api.get<Comentario[]>(`/noticias/${idNoticia}/comentarios`))
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudieron cargar los comentarios.')
    }
  }

  useEffect(() => {
    let activo = true
    ;(async () => {
      try {
        const datos = await api.get<Comentario[]>(`/noticias/${idNoticia}/comentarios`)
        if (activo) setComentarios(datos)
      } catch (e) {
        if (activo) setError(e instanceof ErrorApi ? e.message : 'No se pudieron cargar los comentarios.')
      }
    })()
    return () => {
      activo = false
    }
  }, [idNoticia])

  async function comentar(evento: React.FormEvent) {
    evento.preventDefault()
    setError(null)
    try {
      await api.post(`/noticias/${idNoticia}/comentarios`, { autor, avatar: null, texto })
      setAutor('')
      setTexto('')
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo comentar.')
    }
  }

  return (
    <div className="comentarios">
      {comentarios.map((c) => (
        <div key={c.id} className="comentario">
          <span className="sutil">{c.autor}:</span> {c.texto}
        </div>
      ))}
      <form className="formulario-en-linea" onSubmit={comentar}>
        <input placeholder="Autor" value={autor} onChange={(e) => setAutor(e.target.value)} required />
        <input placeholder="Comentario" value={texto} onChange={(e) => setTexto(e.target.value)} required />
        <button type="submit">Enviar</button>
      </form>
      {error && <p className="error">{error}</p>}
    </div>
  )
}
