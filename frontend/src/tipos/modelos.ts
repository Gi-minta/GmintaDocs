// Tipos que reflejan los DTO expuestos por la API .NET.

/** Página de resultados devuelta por los endpoints de listado paginados. */
export interface ResultadoPaginado<T> {
  elementos: T[]
  pagina: number
  tamanoPagina: number
  total: number
  totalPaginas: number
}

export interface UsuarioAutenticado {
  id: string
  userName: string | null
  fullName: string | null
  roles: string[]
}

export interface RespuestaLogin {
  token: string
  expiraEn: string
  usuario: UsuarioAutenticado
}

export interface Usuario {
  id: string
  userName: string | null
  fullName: string | null
  email: string | null
  activo: boolean | null
  roles: string[]
}

export interface Noticia {
  id: number
  titulo: string
  autor: string
  texto: string
  fechaPublicacion: string
}

export interface Comentario {
  id: number
  idNoticia: number
  autor: string
  texto: string
  fechaPublicacion: string
}

export interface Tarea {
  id: number
  idWorkflow: number
  asunto: string
  estado: number
  prioridad: string
  responsable: string
  fechaVencimiento: string
  fechaEjecucion: string | null
}

export interface Rol {
  id: string
  nombre: string
}

export interface Empresa {
  id: number
  razonSocial: string
  nit: string
  direccion: string | null
  ciudad: string | null
  url: string | null
  email: string | null
  telefono: string | null
  notas: string | null
}

export interface Sucursal {
  id: number
  idEmpresa: number
  codigo: string
  nombre: string
  direccion: string
  telefono: string
  activa: boolean
}

export interface Formulario {
  id: number
  codigo: string
  tabla: string
  nombre: string
  descripcion: string | null
  estado: number
}

export interface Campo {
  id: number
  idFormulario: number
  orden: number
  nombre: string
  columna: string
  tipoDato: number
  requerido: boolean
}

export interface Directorio {
  id: number
  idFormulario: number
  idDirectorioPadre: number
  codigo: string
  nombre: string
  idEstado: number
}

export interface Proceso {
  id: number
  nombre: string
  descripcion: string
  idFormulario: number
  estado: number
  version: string
}

export interface Paso {
  id: number
  idProceso: number
  numero: number
  descripcion: string
  prioridad: string
  plazo: number
  unidadPlazo: string
}

export interface TipoDocumento {
  id: number
  codigo: string
  nombre: string
  controlaVersion: boolean
  diasVigencia: number
  controlaVigencia: boolean
}

export interface Archivo {
  id: number
  nombre: string
  extension: string
  directorio: string
  estado: number
  version: string
  esVersionActual: boolean
  idTipoDocumento: number
  bytes: number
}

export interface Plantilla {
  id: number
  codigo: string
  nombre: string
  contenido: string
}

export interface PlantillaFormato {
  id: number
  codigo: string
  nombre: string
  estado: number
  idFormulario: number
}

export interface CategoriaReporte {
  id: number
  codigo: string
  categoria: string
  descripcion: string | null
}

export interface Reporte {
  id: number
  idCategoria: number
  codigo: string
  reporte: string
  url: string
  descripcion: string | null
}
