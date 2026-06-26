import { test, expect } from '../fixtures/auth.fixture.js';
import {
  nuevaEmpresa,
  nuevoTipoDocumento,
  nuevoArchivo,
} from '../helpers/test-data.js';
import type { ApiClient } from '../helpers/api-client.js';

/**
 * Gestión Documental (CU-025 a CU-032). Módulo GestionDocumental, BD por empresa
 * (policy Gestion). Rutas absolutas por acción (no hay [Route] de clase):
 *  - GET    /api/tipos-documento?pagina&tamano  → ResultadoPaginado<TipoDocumentoDto>
 *  - POST   /api/tipos-documento  { codigo, nombre, controlaVersion, diasVigencia, controlaVigencia } → 201 { id }
 *  - PUT    /api/tipos-documento/{id}  { nombre, controlaVersion, diasVigencia, controlaVigencia } → 204
 *  - DELETE /api/tipos-documento/{id}                                            → 204
 *  - POST   /api/archivos  { nombre, extension, directorio, idTipoDocumento, bytes, version?, descripcion? } → 201 { id }
 *  - GET    /api/archivos/{id}                                                   → ArchivoDto | 404
 *  - GET    /api/archivos/directorio/{directorio}                               → ArchivoDto[]
 *  - DELETE /api/archivos/{id}                                                   → 204
 *
 * TipoDocumentoDto = { id, codigo, nombre, controlaVersion, diasVigencia, controlaVigencia }
 * ArchivoDto       = { id, nombre, extension, directorio, estado, version, esVersionActual, idTipoDocumento, bytes }
 *
 * `tipo_documento` no tiene GET-por-id (solo listado paginado). Para que las
 * aserciones de listado sean deterministas, esos tests usan una empresa nueva
 * (BD limpia). Los archivos sí tienen GET-por-id y listado acotado por la
 * etiqueta `directorio`, así que corren sobre la empresa inicial.
 */
interface TipoDocumentoDto {
  id: number;
  codigo: string;
  nombre: string;
  controlaVersion: boolean;
  diasVigencia: number;
  controlaVigencia: boolean;
}

/** Crea y aprovisiona una empresa nueva; devuelve un cliente apuntando a su BD limpia. */
async function clienteEmpresaLimpia(apiAdmin: ApiClient): Promise<ApiClient> {
  const creacion = await apiAdmin.post('/api/empresas', nuevaEmpresa());
  expect(creacion.status(), await creacion.text()).toBe(201);
  const idEmpresa = ((await creacion.json()) as { id: number }).id;
  const aprov = await apiAdmin.post(`/api/empresas/${idEmpresa}/aprovisionar`);
  expect(aprov.status(), await aprov.text()).toBe(200);
  return apiAdmin.con({ idEmpresa });
}

async function crearTipo(api: ApiClient): Promise<{ id: number; datos: ReturnType<typeof nuevoTipoDocumento> }> {
  const datos = nuevoTipoDocumento();
  const creacion = await api.post('/api/tipos-documento', datos);
  expect(creacion.status(), await creacion.text()).toBe(201);
  const { id } = (await creacion.json()) as { id: number };
  expect(id).toBeGreaterThan(0);
  return { id, datos };
}

async function listarTipos(api: ApiClient): Promise<TipoDocumentoDto[]> {
  const respuesta = await api.get('/api/tipos-documento?pagina=1&tamano=100');
  expect(respuesta.ok()).toBeTruthy();
  return ((await respuesta.json()) as { elementos: TipoDocumentoDto[] }).elementos;
}

test.describe('Gestión Documental — Tipos de documento', () => {
  test('CU-031 · Crear tipo de documento y verlo en el listado de una empresa nueva', async ({ apiAdmin }) => {
    const cliente = await clienteEmpresaLimpia(apiAdmin);
    const { datos } = await crearTipo(cliente);

    const tipos = await listarTipos(cliente);
    expect(tipos).toHaveLength(1);
    const creado = tipos[0];
    expect(creado.codigo).toBe(datos.codigo);
    expect(creado.nombre).toBe(datos.nombre);
    expect(creado.controlaVersion).toBe(true);
    expect(creado.diasVigencia).toBe(365);
    expect(creado.controlaVigencia).toBe(true);
  });

  test('CU-031b · Actualizar tipo de documento se refleja en el listado', async ({ apiAdmin }) => {
    const cliente = await clienteEmpresaLimpia(apiAdmin);
    const { id } = await crearTipo(cliente);

    const actualizacion = await cliente.put(`/api/tipos-documento/${id}`, {
      nombre: 'Tipo Doc Renombrado',
      controlaVersion: false,
      diasVigencia: 30,
      controlaVigencia: false,
    });
    expect(actualizacion.status(), await actualizacion.text()).toBe(204);

    const [tipo] = await listarTipos(cliente);
    expect(tipo.nombre).toBe('Tipo Doc Renombrado');
    expect(tipo.controlaVersion).toBe(false);
    expect(tipo.diasVigencia).toBe(30);
  });

  test('CU-031c · Eliminar tipo de documento lo quita del listado', async ({ apiAdmin }) => {
    const cliente = await clienteEmpresaLimpia(apiAdmin);
    const { id } = await crearTipo(cliente);

    const eliminacion = await cliente.del(`/api/tipos-documento/${id}`);
    expect(eliminacion.status(), await eliminacion.text()).toBe(204);

    expect(await listarTipos(cliente)).toHaveLength(0);
  });

  test('CU-031d · Crear tipo de documento requiere autenticación (401 sin token)', async ({ apiAnonimo }) => {
    const respuesta = await apiAnonimo.post('/api/tipos-documento', nuevoTipoDocumento());
    expect(respuesta.status()).toBe(401);
  });
});

test.describe('Gestión Documental — Archivos', () => {
  test('CU-025 · Registrar archivo (requiere tipo) y consultarlo por id', async ({ apiAdmin }) => {
    const { id: idTipo } = await crearTipo(apiAdmin);
    const datos = nuevoArchivo(idTipo);

    const creacion = await apiAdmin.post('/api/archivos', datos);
    expect(creacion.status(), await creacion.text()).toBe(201);
    const { id } = (await creacion.json()) as { id: number };

    const consulta = await apiAdmin.get(`/api/archivos/${id}`);
    expect(consulta.ok()).toBeTruthy();
    const archivo = (await consulta.json()) as {
      nombre: string;
      extension: string;
      directorio: string;
      idTipoDocumento: number;
      bytes: number;
      version: string;
      esVersionActual: boolean;
    };
    expect(archivo.nombre).toBe(datos.nombre);
    expect(archivo.extension).toBe(datos.extension);
    expect(archivo.directorio).toBe(datos.directorio);
    expect(archivo.idTipoDocumento).toBe(idTipo);
    expect(archivo.bytes).toBe(datos.bytes);
    // Primer registro = versión actual (RN-011, control de versiones)
    expect(archivo.esVersionActual).toBe(true);
    expect(archivo.version).toBeTruthy();
  });

  test('CU-026 · Listar archivos por su directorio devuelve solo los de esa etiqueta', async ({ apiAdmin }) => {
    const { id: idTipo } = await crearTipo(apiAdmin);
    const datos = nuevoArchivo(idTipo); // directorio único por test

    const creacion = await apiAdmin.post('/api/archivos', datos);
    expect(creacion.status()).toBe(201);

    const listado = await apiAdmin.get(`/api/archivos/directorio/${datos.directorio}`);
    expect(listado.ok()).toBeTruthy();
    const archivos = (await listado.json()) as Array<{ directorio: string; idTipoDocumento: number }>;
    expect(archivos).toHaveLength(1);
    expect(archivos[0].directorio).toBe(datos.directorio);
    expect(archivos[0].idTipoDocumento).toBe(idTipo);
  });

  test('CU-027 · Eliminar archivo lo deja inaccesible (404)', async ({ apiAdmin }) => {
    const { id: idTipo } = await crearTipo(apiAdmin);
    const creacion = await apiAdmin.post('/api/archivos', nuevoArchivo(idTipo));
    expect(creacion.status()).toBe(201);
    const { id } = (await creacion.json()) as { id: number };

    const eliminacion = await apiAdmin.del(`/api/archivos/${id}`);
    expect(eliminacion.status(), await eliminacion.text()).toBe(204);

    const consulta = await apiAdmin.get(`/api/archivos/${id}`);
    expect(consulta.status()).toBe(404);
  });
});
