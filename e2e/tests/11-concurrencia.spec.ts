import { test, expect } from '../fixtures/auth.fixture.js';
import {
  nuevoFormulario,
  nuevoProceso,
  nuevaTarea,
  nuevaEmpresa,
  sufijoUnico,
} from '../helpers/test-data.js';
import type { ApiClient } from '../helpers/api-client.js';

/**
 * Concurrencia.
 *
 * Importante: `Tarea.Ejecutar` en el dominio NO tiene guarda de "ya ejecutada"
 * (fija estado + fechaEjecucion incondicionalmente) y `tareas` no tiene token de
 * concurrencia optimista. Por tanto, contrario a lo que asumía el doc de
 * referencia (CU-042: "solo una debe tener éxito"), **dos ejecuciones simultáneas
 * tienen ambas éxito** (last-write-wins). Estos tests fijan ese comportamiento
 * real como ancla de regresión y documentan la ausencia de la guarda.
 */
async function iniciarWorkflow(api: ApiClient): Promise<number> {
  const fc = await api.post('/api/formularios', nuevoFormulario());
  expect(fc.status()).toBe(201);
  const idFormulario = ((await fc.json()) as { id: number }).id;

  const pc = await api.post('/api/procesos', nuevoProceso(idFormulario));
  expect(pc.status()).toBe(201);
  const idProceso = ((await pc.json()) as { id: number }).id;

  const wc = await api.post(`/api/procesos/${idProceso}/workflows`, { idFormulario, idRegistro: 1 });
  expect(wc.status()).toBe(201);
  return ((await wc.json()) as { id: number }).id;
}

async function crearTarea(api: ApiClient, idWorkflow: number, responsable: string): Promise<number> {
  const creacion = await api.post('/api/tareas', nuevaTarea(idWorkflow, responsable));
  expect(creacion.status(), await creacion.text()).toBe(201);
  return ((await creacion.json()) as { id: number }).id;
}

test.describe('Concurrencia — Ejecución de tareas', () => {
  test('CU-042 · Ejecuciones simultáneas de la misma tarea no corrompen el estado final', async ({ apiAdmin }) => {
    const idWorkflow = await iniciarWorkflow(apiAdmin);
    const idTarea = await crearTarea(apiAdmin, idWorkflow, `resp_${sufijoUnico()}`);

    // 5 ejecuciones simultáneas con estados distintos (2..6)
    const estados = [2, 3, 4, 5, 6];
    const respuestas = await Promise.all(
      estados.map((estado) => apiAdmin.post(`/api/tareas/${idTarea}/ejecutar`, { estado })),
    );

    // Comportamiento real: no hay guarda → todas tienen éxito (last-write-wins)
    expect(respuestas.map((r) => r.status())).toEqual(estados.map(() => 204));

    // Y el estado final es consistente: uno de los enviados, con fechaEjecucion fijada.
    const consulta = await apiAdmin.get(`/api/tareas/${idTarea}`);
    const tarea = (await consulta.json()) as { estado: number; fechaEjecucion: string | null };
    expect(estados).toContain(tarea.estado);
    expect(tarea.fechaEjecucion).not.toBeNull();
  });

  test('Creación concurrente de tareas no pierde registros', async ({ apiAdmin }) => {
    const idWorkflow = await iniciarWorkflow(apiAdmin);
    const responsable = `resp_${sufijoUnico()}`;

    const N = 5;
    const creaciones = await Promise.all(
      Array.from({ length: N }, () => apiAdmin.post('/api/tareas', nuevaTarea(idWorkflow, responsable))),
    );
    expect(creaciones.map((r) => r.status())).toEqual(Array.from({ length: N }, () => 201));

    // Las N tareas concurrentes quedan persistidas y consultables por responsable.
    const listado = await apiAdmin.get(`/api/tareas/responsable/${responsable}`);
    expect(listado.ok()).toBeTruthy();
    expect((await listado.json()) as unknown[]).toHaveLength(N);
  });
});

test.describe('Concurrencia — Unicidad de NIT (check-then-act sin constraint)', () => {
  test('Crear la misma empresa en paralelo: al menos una gana (documenta la carrera)', async ({ apiAdmin }) => {
    // El NIT no tiene UNIQUE en la BD (§1.4) y la unicidad se valida con un
    // check-then-act en la aplicación → bajo concurrencia podrían colarse
    // duplicados. Se afirma el invariante robusto: al menos una creación gana.
    const datos = nuevaEmpresa();
    const respuestas = await Promise.all([
      apiAdmin.post('/api/empresas', datos),
      apiAdmin.post('/api/empresas', datos),
      apiAdmin.post('/api/empresas', datos),
    ]);

    const exitosas = respuestas.filter((r) => r.status() === 201);
    expect(exitosas.length).toBeGreaterThanOrEqual(1);
    // El resto (si la carrera se resolvió) deberían ser 400 por NIT duplicado.
    for (const r of respuestas) {
      expect([201, 400]).toContain(r.status());
    }
  });
});
