import { useState } from 'react'
import { api, ErrorApi } from '../api/cliente'
import { useLista } from '../hooks/useLista'
import { useListaPaginada } from '../hooks/useListaPaginada'
import { Paginacion } from '../componentes/Paginacion'
import type { Empresa, Sucursal } from '../tipos/modelos'

export function Empresas() {
  const { datos, cargando, error, recargar, setError, pagina, setPagina, total, totalPaginas } =
    useListaPaginada<Empresa>('/empresas')
  const [razonSocial, setRazonSocial] = useState('')
  const [nit, setNit] = useState('')
  const [seleccionada, setSeleccionada] = useState<number | null>(null)
  const [editId, setEditId] = useState<number | null>(null)
  const [editRazon, setEditRazon] = useState('')

  async function crear(evento: React.FormEvent) {
    evento.preventDefault()
    try {
      await api.post('/empresas', { razonSocial, nit })
      setRazonSocial('')
      setNit('')
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo crear la empresa.')
    }
  }

  function iniciarEdicion(em: Empresa) {
    setEditId(em.id)
    setEditRazon(em.razonSocial)
  }

  async function guardar(em: Empresa) {
    try {
      await api.put(`/empresas/${em.id}`, {
        razonSocial: editRazon,
        direccion: em.direccion,
        ciudad: em.ciudad,
        url: em.url,
        email: em.email,
        telefono: em.telefono,
        notas: em.notas,
      })
      setEditId(null)
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo actualizar la empresa.')
    }
  }

  return (
    <section>
      <h1>Empresas</h1>

      <form className="tarjeta formulario-en-linea" onSubmit={crear}>
        <input placeholder="Razón social" value={razonSocial} onChange={(e) => setRazonSocial(e.target.value)} required />
        <input placeholder="NIT" value={nit} onChange={(e) => setNit(e.target.value)} required />
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
              <th>Razón social</th>
              <th>NIT</th>
              <th>Ciudad</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {datos.map((em) => (
              <tr key={em.id}>
                <td>{em.id}</td>
                <td>
                  {editId === em.id ? (
                    <input value={editRazon} onChange={(e) => setEditRazon(e.target.value)} />
                  ) : (
                    em.razonSocial
                  )}
                </td>
                <td>{em.nit}</td>
                <td>{em.ciudad}</td>
                <td>
                  <div className="acciones">
                    {editId === em.id ? (
                      <>
                        <button className="enlace" onClick={() => guardar(em)}>
                          Guardar
                        </button>
                        <button className="enlace" onClick={() => setEditId(null)}>
                          Cancelar
                        </button>
                      </>
                    ) : (
                      <>
                        <button className="enlace" onClick={() => setSeleccionada(seleccionada === em.id ? null : em.id)}>
                          {seleccionada === em.id ? 'Ocultar sucursales' : 'Sucursales'}
                        </button>
                        <button className="enlace" onClick={() => iniciarEdicion(em)}>
                          Editar
                        </button>
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

      {seleccionada !== null && <Sucursales idEmpresa={seleccionada} />}
    </section>
  )
}

function Sucursales({ idEmpresa }: { idEmpresa: number }) {
  const { datos, error, recargar, setError } = useLista<Sucursal>(`/empresas/${idEmpresa}/sucursales`)
  const [codigo, setCodigo] = useState('')
  const [nombre, setNombre] = useState('')
  const [direccion, setDireccion] = useState('')
  const [telefono, setTelefono] = useState('')

  async function crear(evento: React.FormEvent) {
    evento.preventDefault()
    try {
      await api.post(`/empresas/${idEmpresa}/sucursales`, { codigo, nombre, direccion, telefono })
      setCodigo('')
      setNombre('')
      setDireccion('')
      setTelefono('')
      await recargar()
    } catch (e) {
      setError(e instanceof ErrorApi ? e.message : 'No se pudo crear la sucursal.')
    }
  }

  return (
    <div className="tarjeta" style={{ marginTop: '1rem' }}>
      <h3>Sucursales de la empresa #{idEmpresa}</h3>
      <form className="formulario-en-linea" onSubmit={crear}>
        <input placeholder="Código" value={codigo} onChange={(e) => setCodigo(e.target.value)} required />
        <input placeholder="Nombre" value={nombre} onChange={(e) => setNombre(e.target.value)} required />
        <input placeholder="Dirección" value={direccion} onChange={(e) => setDireccion(e.target.value)} required />
        <input placeholder="Teléfono" value={telefono} onChange={(e) => setTelefono(e.target.value)} required />
        <button type="submit">Añadir</button>
      </form>
      {error && <p className="error">{error}</p>}
      {datos.length === 0 ? (
        <p className="sutil">Sin sucursales.</p>
      ) : (
        <ul>
          {datos.map((s) => (
            <li key={s.id}>
              <strong>{s.codigo}</strong> — {s.nombre} ({s.activa ? 'activa' : 'inactiva'})
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
