import { test, expect } from '../fixtures/auth.fixture.js';
import {
  nuevaEmpresa,
  nuevoFormulario,
  nuevoCampo,
  nuevoDirectorio,
} from '../helpers/test-data.js';
import type { ApiClient } from '../helpers/api-client.js';

/**
 * Formularios, Campos y Directorios (CU-013 a CU-024). Módulos AdminFormularios
 * y AdminDirectorios, en la BD por empresa (policy Gestion). Se opera sobre la
 * empresa inicial (id 1, ya aprovisionada) salvo el listado paginado, que usa
 * una empresa nueva para ser determinista.
 *
 * Contrato real:
 *  - POST /api/formularios                { codigo, tabla, nombre }            → 201 { id }
 *  - GET  /api/formularios/{id}                                                → FormularioDto
 *  - PUT  /api/formularios/{id}           { nombre, descripcion? }             → 204
 *  - DELETE /api/formularios/{id}                                              → 204
 *  - GET  /api/formularios/{id}/campos                                         → CampoDto[]
 *  - POST /api/formularios/{id}/campos    { orden, nombre, columna, tipoDato, longDato, control, requerido } → 201 { id }
 *  - POST /api/directorios                { idFormulario, idDirectorioPadre, codigo, nombre } → 201 { id }
 *  - GET  /api/directorios/{id}                                                → DirectorioDto
 *  - GET  /api/directorios/formulario/{idFormulario}                          → DirectorioDto[]
 *
 * FormularioDto = { id, codigo, tabla, nombre, descripcion, estado }
 * CampoDto      = { id, idFormulario, orden, nombre, columna, tipoDato, requerido }
 * DirectorioDto = { id, idFormulario, idDirectorioPadre, codigo, nombre, idEstado }
 */
interface FormularioDto {
  id: number;
  codigo: string;
  tabla: string;
  nombre: string;
  descripcion?: string;
  estado: number;
}

async function crearFormulario(api: ApiClient): Promise<{ id: number; datos: ReturnType<typeof nuevoFormulario> }> {
  const datos = nuevoFormulario();
  const creacion = await api.post('/api/formularios', datos);
  expect(creacion.status(), await creacion.text()).toBe(201);
  const { id } = (await creacion.json()) as { id: number };
  expect(id).toBeGreaterThan(0);
  return { id, datos };
}

test.describe('Formularios — CRUD', () => {
  test('CU-013 · Crear formulario y consultarlo por id', async ({ apiAdmin }) => {
    const { id, datos } = await crearFormulario(apiAdmin);

    const consulta = await apiAdmin.get(`/api/formularios/${id}`);
    expect(consulta.ok()).toBeTruthy();
    const formulario = (await consulta.json()) as FormularioDto;
    expect(formulario.codigo).toBe(datos.codigo);
    expect(formulario.tabla).toBe(datos.tabla);
    expect(formulario.nombre).toBe(datos.nombre);
  });

  test('CU-014 · Actualizar nombre y descripción del formulario', async ({ apiAdmin }) => {
    const { id } = await crearFormulario(apiAdmin);

    const actualizacion = await apiAdmin.put(`/api/formularios/${id}`, {
      nombre: 'Formulario Renombrado E2E',
      descripcion: 'Descripción actualizada',
    });
    expect(actualizacion.status(), await actualizacion.text()).toBe(204);

    const consulta = await apiAdmin.get(`/api/formularios/${id}`);
    const formulario = (await consulta.json()) as FormularioDto;
    expect(formulario.nombre).toBe('Formulario Renombrado E2E');
    expect(formulario.descripcion).toBe('Descripción actualizada');
  });

  test('CU-015 · Eliminar formulario lo deja inaccesible (404)', async ({ apiAdmin }) => {
    const { id } = await crearFormulario(apiAdmin);

    const eliminacion = await apiAdmin.del(`/api/formularios/${id}`);
    expect(eliminacion.status(), await eliminacion.text()).toBe(204);

    const consulta = await apiAdmin.get(`/api/formularios/${id}`);
    expect(consulta.status()).toBe(404);
  });

  test('CU-013b · Crear formulario requiere autenticación (401 sin token)', async ({ apiAnonimo }) => {
    const respuesta = await apiAnonimo.post('/api/formularios', nuevoFormulario());
    expect(respuesta.status()).toBe(401);
  });
});

test.describe('Formularios — Campos', () => {
  test('CU-016 · Agregar campos a un formulario y listarlos en orden', async ({ apiAdmin }) => {
    const { id } = await crearFormulario(apiAdmin);

    // Agregar dos campos con orden 1 y 2
    for (const orden of [1, 2]) {
      const campo = nuevoCampo(orden);
      const creacion = await apiAdmin.post(`/api/formularios/${id}/campos`, campo);
      expect(creacion.status(), await creacion.text()).toBe(201);
    }

    // El listado por formulario está acotado a ESTE formulario → determinista
    const listado = await apiAdmin.get(`/api/formularios/${id}/campos`);
    expect(listado.ok()).toBeTruthy();
    const campos = (await listado.json()) as Array<{ idFormulario: number; orden: number; requerido: boolean }>;
    expect(campos).toHaveLength(2);
    expect(campos.map((c) => c.orden).sort()).toEqual([1, 2]);
    expect(campos.every((c) => c.idFormulario === id)).toBe(true);
  });
});

test.describe('Directorios', () => {
  test('CU-020 · Crear directorio bajo un formulario y consultarlo por id', async ({ apiAdmin }) => {
    const { id: idFormulario } = await crearFormulario(apiAdmin);
    const datos = nuevoDirectorio(idFormulario);

    const creacion = await apiAdmin.post('/api/directorios', datos);
    expect(creacion.status(), await creacion.text()).toBe(201);
    const { id } = (await creacion.json()) as { id: number };

    const consulta = await apiAdmin.get(`/api/directorios/${id}`);
    expect(consulta.ok()).toBeTruthy();
    const directorio = (await consulta.json()) as {
      idFormulario: number;
      codigo: string;
      nombre: string;
      idDirectorioPadre: number;
    };
    expect(directorio.idFormulario).toBe(idFormulario);
    expect(directorio.codigo).toBe(datos.codigo);
    expect(directorio.nombre).toBe(datos.nombre);
  });

  test('CU-021 · Listar directorios por formulario devuelve solo los suyos', async ({ apiAdmin }) => {
    const { id: idFormulario } = await crearFormulario(apiAdmin);

    const datos = nuevoDirectorio(idFormulario);
    const creacion = await apiAdmin.post('/api/directorios', datos);
    expect(creacion.status()).toBe(201);

    const listado = await apiAdmin.get(`/api/directorios/formulario/${idFormulario}`);
    expect(listado.ok()).toBeTruthy();
    const directorios = (await listado.json()) as Array<{ idFormulario: number; codigo: string }>;
    expect(directorios).toHaveLength(1);
    expect(directorios[0].idFormulario).toBe(idFormulario);
    expect(directorios[0].codigo).toBe(datos.codigo);
  });
});

test.describe('Formularios — Listado paginado', () => {
  test('CU-019 · En una empresa nueva el listado refleja exactamente los formularios creados', async ({ apiAdmin }) => {
    // Empresa nueva → BD vacía y determinista
    const creacionEmpresa = await apiAdmin.post('/api/empresas', nuevaEmpresa());
    expect(creacionEmpresa.status(), await creacionEmpresa.text()).toBe(201);
    const idEmpresa = ((await creacionEmpresa.json()) as { id: number }).id;
    const aprov = await apiAdmin.post(`/api/empresas/${idEmpresa}/aprovisionar`);
    expect(aprov.status(), await aprov.text()).toBe(200);

    const cliente = apiAdmin.con({ idEmpresa });

    // Arranca vacío
    const vacio = await cliente.get('/api/formularios?pagina=1&tamano=20');
    expect(((await vacio.json()) as { total: number }).total).toBe(0);

    // Crear dos formularios
    await crearFormulario(cliente);
    await crearFormulario(cliente);

    const listado = await cliente.get('/api/formularios?pagina=1&tamano=20');
    expect(listado.ok()).toBeTruthy();
    const pagina = (await listado.json()) as { elementos: unknown[]; total: number; totalPaginas: number };
    expect(pagina.total).toBe(2);
    expect(pagina.elementos).toHaveLength(2);
    expect(pagina.totalPaginas).toBe(1);
  });
});
