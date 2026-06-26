import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { ProveedorAuth } from './auth/AuthContext'
import { RutaProtegida } from './auth/RutaProtegida'
import { Layout } from './componentes/Layout'
import { Login } from './paginas/Login'
import { Panel } from './paginas/Panel'
import { Noticias } from './paginas/Noticias'
import { Tareas } from './paginas/Tareas'
import { Usuarios } from './paginas/Usuarios'
import { Roles } from './paginas/Roles'
import { Empresas } from './paginas/Empresas'
import { Formularios } from './paginas/Formularios'
import { Directorios } from './paginas/Directorios'
import { Procesos } from './paginas/Procesos'
import { Documentos } from './paginas/Documentos'
import { Plantillas } from './paginas/Plantillas'
import { Reportes } from './paginas/Reportes'

export default function App() {
  return (
    <ProveedorAuth>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route element={<RutaProtegida />}>
            <Route element={<Layout />}>
              <Route index element={<Panel />} />
              <Route path="empresas" element={<Empresas />} />
              <Route path="usuarios" element={<Usuarios />} />
              <Route path="roles" element={<Roles />} />
              <Route path="formularios" element={<Formularios />} />
              <Route path="directorios" element={<Directorios />} />
              <Route path="procesos" element={<Procesos />} />
              <Route path="documentos" element={<Documentos />} />
              <Route path="plantillas" element={<Plantillas />} />
              <Route path="reportes" element={<Reportes />} />
              <Route path="tareas" element={<Tareas />} />
              <Route path="noticias" element={<Noticias />} />
            </Route>
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </ProveedorAuth>
  )
}
