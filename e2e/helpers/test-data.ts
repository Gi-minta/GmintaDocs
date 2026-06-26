/**
 * Datos de configuración y generadores de fixtures para las pruebas E2E.
 *
 * Los valores por defecto coinciden con el bootstrap del backend:
 *  - Admin sembrado: `admin` / `admin123` (appsettings.json → Seguridad:Admin).
 *  - Empresa inicial aprovisionada en el arranque con id `1`
 *    (Persistencia:AprovisionarEmpresaInicial = true, Tenants:1).
 */
export const CONFIG = {
  admin: {
    userName: process.env.ADMIN_USERNAME ?? 'admin',
    contrasena: process.env.ADMIN_PASSWORD ?? 'admin123',
  },
  /** Empresa sembrada al arrancar; su token sirve para operar el núcleo admin. */
  empresaInicialId: Number(process.env.EMPRESA_INICIAL_ID ?? 1),
} as const;

let contador = 0;

/**
 * Sufijo único: timestamp (varía entre corridas) + contador incremental (garantiza
 * unicidad dentro de la misma corrida aunque dos llamadas caigan en el mismo ms).
 */
export function sufijoUnico(): string {
  return `${Date.now()}${(contador++).toString().padStart(3, '0')}`;
}

/**
 * Payload real de creación de empresa: el endpoint POST /api/empresas solo
 * acepta { razonSocial, nit } (ver EmpresasController.CrearEmpresaRequest).
 * El resto de campos (direccion, ciudad, email, …) se asignan vía PUT.
 */
export function nuevaEmpresa(sufijo = sufijoUnico()) {
  return {
    razonSocial: `Empresa Test ${sufijo}`,
    // El NIT debe ser único (la API rechaza duplicados); usar la cola variable del sufijo.
    nit: `900${sufijo.slice(-7)}-1`,
  };
}

/**
 * Payload de creación de sucursal: POST /api/empresas/{id}/sucursales acepta
 * { codigo, nombre, direccion, telefono } (ver EmpresasController.CrearSucursalRequest).
 */
export function nuevaSucursal(sufijo = sufijoUnico()) {
  return {
    codigo: `SUC${sufijo.slice(-6)}`,
    nombre: `Sucursal Test ${sufijo}`,
    direccion: 'Calle Sucursal 123',
    telefono: '7654321',
  };
}

/**
 * Payload de creación de usuario: POST /api/usuarios acepta
 * { userName, fullName?, email?, contrasena? } (ver UsuariosController.CrearUsuarioRequest).
 */
export function nuevoUsuario(sufijo = sufijoUnico()) {
  return {
    userName: `usuario.test.${sufijo}`,
    fullName: 'Usuario De Prueba E2E',
    email: `usuario${sufijo}@gmintadocs.test`,
    contrasena: `Clave!${sufijo}`,
  };
}

/** Payload de creación de rol: POST /api/roles acepta { nombre }. */
export function nuevoRol(sufijo = sufijoUnico()) {
  return { nombre: `Rol Test ${sufijo}` };
}

/**
 * Payload de publicación de noticia (módulo Colaboración, BD por empresa):
 * POST /api/noticias acepta { titulo, autor, avatar?, texto }
 * (ver NoticiasController.PublicarNoticiaRequest).
 */
export function nuevaNoticia(sufijo = sufijoUnico()) {
  return {
    titulo: `Noticia Test ${sufijo}`,
    autor: 'Autor E2E',
    texto: 'Contenido de la noticia de prueba E2E',
  };
}

/**
 * Payload de creación de formulario (módulo AdminFormularios, BD por empresa):
 * POST /api/formularios acepta { codigo, tabla, nombre }
 * (ver FormulariosController.CrearFormularioRequest).
 */
export function nuevoFormulario(sufijo = sufijoUnico()) {
  return {
    codigo: `F${sufijo.slice(-8)}`,
    tabla: `tabla_test_${sufijo.slice(-6)}`,
    nombre: `Formulario Test ${sufijo}`,
  };
}

/**
 * Payload para agregar un campo a un formulario:
 * POST /api/formularios/{id}/campos acepta
 * { orden, nombre, columna, tipoDato, longDato, control, requerido }.
 * tipoDato/control son códigos catalogados (1 = valor por defecto razonable).
 */
export function nuevoCampo(orden: number, sufijo = sufijoUnico()) {
  return {
    orden,
    nombre: `Campo ${orden} ${sufijo}`,
    columna: `col_${orden}_${sufijo.slice(-6)}`,
    tipoDato: 1,
    longDato: 100,
    control: 1,
    requerido: true,
  };
}

/**
 * Payload de creación de directorio (módulo AdminDirectorios, BD por empresa):
 * POST /api/directorios acepta { idFormulario, idDirectorioPadre, codigo, nombre }.
 * idDirectorioPadre = 0 → directorio raíz.
 */
export function nuevoDirectorio(idFormulario: number, sufijo = sufijoUnico()) {
  return {
    idFormulario,
    idDirectorioPadre: 0,
    codigo: `DIR${sufijo.slice(-8)}`,
    nombre: `Directorio Test ${sufijo}`,
  };
}

/**
 * Payload de creación de tipo de documento (módulo GestionDocumental, BD por empresa):
 * POST /api/tipos-documento acepta { codigo, nombre, controlaVersion, diasVigencia, controlaVigencia }.
 */
export function nuevoTipoDocumento(sufijo = sufijoUnico()) {
  return {
    codigo: `TD${sufijo.slice(-8)}`,
    nombre: `Tipo Doc Test ${sufijo}`,
    controlaVersion: true,
    diasVigencia: 365,
    controlaVigencia: true,
  };
}

/**
 * Payload de registro de archivo:
 * POST /api/archivos acepta { nombre, extension, directorio, idTipoDocumento, bytes, version?, descripcion? }.
 * `directorio` es una etiqueta de texto libre (columna archivos.directorio), no el id
 * de la tabla `directorios`; usar una única por test acota el listado por directorio.
 */
export function nuevoArchivo(idTipoDocumento: number, sufijo = sufijoUnico()) {
  return {
    nombre: `archivo-test-${sufijo.slice(-6)}`,
    extension: 'pdf',
    directorio: `dir-${sufijo}`,
    idTipoDocumento,
    bytes: 102400,
    version: '1',
    descripcion: 'Archivo de prueba E2E',
  };
}

/**
 * Payload de creación de proceso (módulo Workflow, BD por empresa):
 * POST /api/procesos acepta { nombre, descripcion?, idFormulario, version? }.
 */
export function nuevoProceso(idFormulario: number, sufijo = sufijoUnico()) {
  return {
    nombre: `Proceso Test ${sufijo}`,
    descripcion: 'Proceso de prueba E2E',
    idFormulario,
    version: '1.0',
  };
}

/**
 * Payload para agregar un paso a un proceso:
 * POST /api/procesos/{id}/pasos acepta { numero, descripcion, prioridad?, plazo, unidadPlazo? }.
 */
export function nuevoPaso(numero: number, sufijo = sufijoUnico()) {
  return {
    numero,
    descripcion: `Paso ${numero} ${sufijo.slice(-4)}`,
    prioridad: 'ALTA',
    plazo: 2,
    unidadPlazo: 'DIAS',
  };
}

/**
 * Payload de creación de categoría de reporte (módulo Reportes, BD por empresa):
 * POST /api/categorias-reporte acepta { codigo, categoria, descripcion? }.
 */
export function nuevaCategoriaReporte(sufijo = sufijoUnico()) {
  return {
    codigo: `CAT${sufijo.slice(-8)}`,
    categoria: `Categoría Test ${sufijo}`,
    descripcion: 'Categoría de prueba E2E',
  };
}

/**
 * Payload de creación de reporte SSRS:
 * POST /api/reportes acepta { idCategoria, codigo, reporte, url, descripcion? }.
 */
export function nuevoReporte(idCategoria: number, sufijo = sufijoUnico()) {
  return {
    idCategoria,
    codigo: `REP${sufijo.slice(-8)}`,
    reporte: `Reporte Test ${sufijo}`,
    url: 'https://ssrs.example.com/reporte-test',
    descripcion: 'Reporte de prueba E2E',
  };
}

/**
 * Payload de creación de tarea (módulo Tareas, BD por empresa):
 * POST /api/tareas acepta { idWorkflow, asunto, descripcion?, prioridad?, paso, responsable, remitente?, fechaVencimiento }.
 * `fechaVencimiento` se envía como ISO (se vincula a DateTime en .NET).
 */
export function nuevaTarea(idWorkflow: number, responsable: string, sufijo = sufijoUnico()) {
  return {
    idWorkflow,
    asunto: `Tarea Test ${sufijo}`,
    descripcion: 'Descripción de tarea de prueba',
    prioridad: 'ALTA',
    paso: 1,
    responsable,
    remitente: 'remitente_test',
    fechaVencimiento: new Date(Date.now() + 86_400_000).toISOString(),
  };
}
