import { test, expect } from '../fixtures/auth.fixture.js';
import {
  nuevoFormulario,
  nuevoProceso,
  nuevoPaso,
  nuevaTarea,
  sufijoUnico,
} from '../helpers/test-data.js';
import type { ApiClient } from '../helpers/api-client.js';

/**
 * Workflow y Tareas (CU-033 a CU-044). Módulos Workflow y Tareas, BD por empresa
 * (policy Gestion). Se opera sobre la empresa inicial (id 1).
 *
 * Cadena real de dependencias (FKs):
 *   formulario → proceso → workflow → tarea
 *
 * Contrato real:
 *  - POST /api/procesos                  { nombre, descripcion?, idFormulario, version? } → 201 { id }
 *  - GET  /api/procesos/{id}                                                              → ProcesoDto
 *  - PUT  /api/procesos/{id}             { nombre, descripcion? }                          → 204
 *  - DELETE /api/procesos/{id}                                                            → 204
 *  - GET  /api/procesos/{id}/pasos                                                        → PasoDto[]
 *  - POST /api/procesos/{id}/pasos       { numero, descripcion, prioridad?, plazo, unidadPlazo? } → 201 { id }
 *  - POST /api/procesos/{id}/workflows   { idFormulario, idRegistro }                     → 201 { id }
 *  - GET  /api/workflows/{id}                                                             → WorkflowDto
 *  - POST /api/tareas                    { idWorkflow, asunto, ..., responsable, fechaVencimiento } → 201 { id }
 *  - GET  /api/tareas/{id}                                                                → TareaDto | 404
 *  - GET  /api/tareas/responsable/{responsable}                                           → TareaDto[]
 *  - POST /api/tareas/{id}/ejecutar      { estado }                                       → 204
 *  - PUT  /api/tareas/{id}               { asunto, ..., responsable, fechaVencimiento }   → 204
 *  - DELETE /api/tareas/{id}                                                              → 204
 *
 * ProcesoDto  = { id, nombre, descripcion, idFormulario, estado, version }
 * PasoDto     = { id, idProceso, numero, descripcion, prioridad, plazo, unidadPlazo }
 * WorkflowDto = { id, idProceso, estado, idFormulario, idRegistro, fechaFinalizacion }
 * TareaDto    = { id, idWorkflow, asunto, estado, prioridad, responsable, fechaVencimiento, fechaEjecucion }
 */
async function crearFormulario(api: ApiClient): Promise<number> {
  const creacion = await api.post('/api/formularios', nuevoFormulario());
  expect(creacion.status(), await creacion.text()).toBe(201);
  return ((await creacion.json()) as { id: number }).id;
}

async function crearProceso(api: ApiClient): Promise<{ id: number; idFormulario: number; datos: ReturnType<typeof nuevoProceso> }> {
  const idFormulario = await crearFormulario(api);
  const datos = nuevoProceso(idFormulario);
  const creacion = await api.post('/api/procesos', datos);
  expect(creacion.status(), await creacion.text()).toBe(201);
  const { id } = (await creacion.json()) as { id: number };
  expect(id).toBeGreaterThan(0);
  return { id, idFormulario, datos };
}

/** Construye un workflow real (formulario → proceso → workflow) y devuelve su id. */
async function iniciarWorkflow(api: ApiClient): Promise<{ idWorkflow: number; idProceso: number; idFormulario: number }> {
  const { id: idProceso, idFormulario } = await crearProceso(api);
  const inicio = await api.post(`/api/procesos/${idProceso}/workflows`, { idFormulario, idRegistro: 1 });
  expect(inicio.status(), await inicio.text()).toBe(201);
  const idWorkflow = ((await inicio.json()) as { id: number }).id;
  return { idWorkflow, idProceso, idFormulario };
}

test.describe('Workflow — Procesos', () => {
  test('CU-033 · Crear proceso y consultarlo por id', async ({ apiAdmin }) => {
    const { id, idFormulario, datos } = await crearProceso(apiAdmin);

    const consulta = await apiAdmin.get(`/api/procesos/${id}`);
    expect(consulta.ok()).toBeTruthy();
    const proceso = (await consulta.json()) as { nombre: string; idFormulario: number; version: string };
    expect(proceso.nombre).toBe(datos.nombre);
    expect(proceso.idFormulario).toBe(idFormulario);
    expect(proceso.version).toBe('1.0');
  });

  test('CU-034 · Actualizar nombre y descripción del proceso', async ({ apiAdmin }) => {
    const { id } = await crearProceso(apiAdmin);

    const actualizacion = await apiAdmin.put(`/api/procesos/${id}`, {
      nombre: 'Proceso Renombrado E2E',
      descripcion: 'Descripción actualizada',
    });
    expect(actualizacion.status(), await actualizacion.text()).toBe(204);

    const consulta = await apiAdmin.get(`/api/procesos/${id}`);
    expect(((await consulta.json()) as { nombre: string }).nombre).toBe('Proceso Renombrado E2E');
  });

  test('CU-037 · Agregar pasos al proceso y listarlos', async ({ apiAdmin }) => {
    const { id } = await crearProceso(apiAdmin);

    for (const numero of [1, 2]) {
      const creacion = await apiAdmin.post(`/api/procesos/${id}/pasos`, nuevoPaso(numero));
      expect(creacion.status(), await creacion.text()).toBe(201);
    }

    const listado = await apiAdmin.get(`/api/procesos/${id}/pasos`);
    expect(listado.ok()).toBeTruthy();
    const pasos = (await listado.json()) as Array<{ idProceso: number; numero: number }>;
    expect(pasos).toHaveLength(2);
    expect(pasos.map((p) => p.numero).sort()).toEqual([1, 2]);
    expect(pasos.every((p) => p.idProceso === id)).toBe(true);
  });

  test('CU-036 · Eliminar proceso lo deja inaccesible (404)', async ({ apiAdmin }) => {
    const { id } = await crearProceso(apiAdmin);

    const eliminacion = await apiAdmin.del(`/api/procesos/${id}`);
    expect(eliminacion.status(), await eliminacion.text()).toBe(204);

    const consulta = await apiAdmin.get(`/api/procesos/${id}`);
    expect(consulta.status()).toBe(404);
  });

  test('CU-033b · Crear proceso requiere autenticación (401 sin token)', async ({ apiAnonimo }) => {
    const respuesta = await apiAnonimo.post('/api/procesos', nuevoProceso(1));
    expect(respuesta.status()).toBe(401);
  });
});

test.describe('Workflow — Instancia', () => {
  test('CU-039 · Iniciar workflow desde un proceso y consultarlo', async ({ apiAdmin }) => {
    const { idWorkflow, idProceso, idFormulario } = await iniciarWorkflow(apiAdmin);

    const consulta = await apiAdmin.get(`/api/workflows/${idWorkflow}`);
    expect(consulta.ok()).toBeTruthy();
    const workflow = (await consulta.json()) as {
      idProceso: number;
      idFormulario: number;
      idRegistro: number;
      fechaFinalizacion: string | null;
    };
    expect(workflow.idProceso).toBe(idProceso);
    expect(workflow.idFormulario).toBe(idFormulario);
    expect(workflow.idRegistro).toBe(1);
    expect(workflow.fechaFinalizacion).toBeNull(); // recién iniciado, no finalizado
  });

  test('CU-040 · Consultar un workflow inexistente devuelve 404', async ({ apiAdmin }) => {
    const respuesta = await apiAdmin.get('/api/workflows/999999999');
    expect(respuesta.status()).toBe(404);
  });
});

test.describe('Tareas', () => {
  test('CU-041 · Crear tarea en un workflow y consultarla por id', async ({ apiAdmin }) => {
    const { idWorkflow } = await iniciarWorkflow(apiAdmin);
    const responsable = `resp_${sufijoUnico()}`;
    const datos = nuevaTarea(idWorkflow, responsable);

    const creacion = await apiAdmin.post('/api/tareas', datos);
    expect(creacion.status(), await creacion.text()).toBe(201);
    const { id } = (await creacion.json()) as { id: number };

    const consulta = await apiAdmin.get(`/api/tareas/${id}`);
    expect(consulta.ok()).toBeTruthy();
    const tarea = (await consulta.json()) as {
      idWorkflow: number;
      asunto: string;
      responsable: string;
      fechaEjecucion: string | null;
    };
    expect(tarea.idWorkflow).toBe(idWorkflow);
    expect(tarea.asunto).toBe(datos.asunto);
    expect(tarea.responsable).toBe(responsable);
    expect(tarea.fechaEjecucion).toBeNull(); // pendiente de ejecutar
  });

  test('CU-044 · Listar tareas por responsable devuelve solo las suyas', async ({ apiAdmin }) => {
    const { idWorkflow } = await iniciarWorkflow(apiAdmin);
    const responsable = `resp_${sufijoUnico()}`;
    const creacion = await apiAdmin.post('/api/tareas', nuevaTarea(idWorkflow, responsable));
    expect(creacion.status()).toBe(201);

    const listado = await apiAdmin.get(`/api/tareas/responsable/${responsable}`);
    expect(listado.ok()).toBeTruthy();
    const tareas = (await listado.json()) as Array<{ responsable: string }>;
    expect(tareas).toHaveLength(1);
    expect(tareas[0].responsable).toBe(responsable);
  });

  test('CU-042 · Ejecutar una tarea fija su estado y su fecha de ejecución', async ({ apiAdmin }) => {
    const { idWorkflow } = await iniciarWorkflow(apiAdmin);
    const creacion = await apiAdmin.post('/api/tareas', nuevaTarea(idWorkflow, `resp_${sufijoUnico()}`));
    const { id } = (await creacion.json()) as { id: number };

    const ejecucion = await apiAdmin.post(`/api/tareas/${id}/ejecutar`, { estado: 2 });
    expect(ejecucion.status(), await ejecucion.text()).toBe(204);

    const consulta = await apiAdmin.get(`/api/tareas/${id}`);
    const tarea = (await consulta.json()) as { estado: number; fechaEjecucion: string | null };
    expect(tarea.estado).toBe(2);
    expect(tarea.fechaEjecucion).not.toBeNull();
  });

  test('CU-043 · Reasignar una tarea cambia su responsable', async ({ apiAdmin }) => {
    const { idWorkflow } = await iniciarWorkflow(apiAdmin);
    const creacion = await apiAdmin.post('/api/tareas', nuevaTarea(idWorkflow, `resp_${sufijoUnico()}`));
    const { id } = (await creacion.json()) as { id: number };
    const nuevoResponsable = `resp_${sufijoUnico()}`;

    const actualizacion = await apiAdmin.put(`/api/tareas/${id}`, {
      asunto: 'Tarea Reasignada',
      responsable: nuevoResponsable,
      fechaVencimiento: new Date(Date.now() + 172_800_000).toISOString(),
    });
    expect(actualizacion.status(), await actualizacion.text()).toBe(204);

    const consulta = await apiAdmin.get(`/api/tareas/${id}`);
    const tarea = (await consulta.json()) as { responsable: string; asunto: string };
    expect(tarea.responsable).toBe(nuevoResponsable);
    expect(tarea.asunto).toBe('Tarea Reasignada');
  });

  test('CU-041b · Eliminar tarea la deja inaccesible (404)', async ({ apiAdmin }) => {
    const { idWorkflow } = await iniciarWorkflow(apiAdmin);
    const creacion = await apiAdmin.post('/api/tareas', nuevaTarea(idWorkflow, `resp_${sufijoUnico()}`));
    const { id } = (await creacion.json()) as { id: number };

    const eliminacion = await apiAdmin.del(`/api/tareas/${id}`);
    expect(eliminacion.status(), await eliminacion.text()).toBe(204);

    const consulta = await apiAdmin.get(`/api/tareas/${id}`);
    expect(consulta.status()).toBe(404);
  });
});
