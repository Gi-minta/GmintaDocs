import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'

const GRUPOS = [
  {
    titulo: 'Núcleo',
    enlaces: [
      { a: '/', texto: 'Panel', fin: true },
      { a: '/empresas', texto: 'Empresas', fin: false },
      { a: '/usuarios', texto: 'Usuarios', fin: false },
      { a: '/roles', texto: 'Roles', fin: false },
    ],
  },
  {
    titulo: 'Gestión',
    enlaces: [
      { a: '/formularios', texto: 'Formularios', fin: false },
      { a: '/directorios', texto: 'Directorios', fin: false },
      { a: '/procesos', texto: 'Procesos', fin: false },
      { a: '/documentos', texto: 'Documentos', fin: false },
      { a: '/plantillas', texto: 'Plantillas', fin: false },
      { a: '/reportes', texto: 'Reportes', fin: false },
      { a: '/tareas', texto: 'Tareas', fin: false },
      { a: '/noticias', texto: 'Noticias', fin: false },
    ],
  },
]

export function Layout() {
  const { usuario, idEmpresa, cerrarSesion } = useAuth()
  const navegar = useNavigate()

  function salir() {
    cerrarSesion()
    navegar('/login', { replace: true })
  }

  return (
    <div className="app">
      <aside className="lateral">
        <div className="marca">GmintaDocs</div>
        <nav>
          {GRUPOS.map((g) => (
            <div key={g.titulo} className="grupo-nav">
              <span className="grupo-titulo">{g.titulo}</span>
              {g.enlaces.map((e) => (
                <NavLink key={e.a} to={e.a} end={e.fin}>
                  {e.texto}
                </NavLink>
              ))}
            </div>
          ))}
        </nav>
      </aside>

      <div className="principal">
        <header className="barra">
          <span className="sutil">Empresa #{idEmpresa}</span>
          <span className="espaciador" />
          <span>{usuario?.fullName ?? usuario?.userName}</span>
          <button className="enlace" onClick={salir}>
            Salir
          </button>
        </header>
        <main className="contenido">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
