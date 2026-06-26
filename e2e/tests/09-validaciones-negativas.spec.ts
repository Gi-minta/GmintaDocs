import { test, expect } from '../fixtures/auth.fixture.js';
import { nuevaEmpresa } from '../helpers/test-data.js';

/**
 * Casos negativos de validación y paginación.
 *
 * Distingue dos tipos de 400:
 *  - Validación de modelo ([ApiController] marca como requeridos los string no-nullable):
 *    campos vacíos/ausentes → 400 (ProblemDetails con `errors`).
 *  - Reglas de negocio del dominio (Result.Fallido) → 400 con `{ error }` y mensaje propio.
 *
 * Paginación: ParametrosDePaginacion normaliza en la respuesta (pagina>=1, 1..100).
 */
test.describe('Validaciones — Empresa', () => {
  test('NIT duplicado es rechazado por regla de negocio (400 con mensaje)', async ({ apiAdmin }) => {
    const datos = nuevaEmpresa();

    const primera = await apiAdmin.post('/api/empresas', datos);
    expect(primera.status(), await primera.text()).toBe(201);

    const duplicada = await apiAdmin.post('/api/empresas', datos); // mismo NIT
    expect(duplicada.status()).toBe(400);
    expect((await duplicada.json()) as { error: string }).toMatchObject({
      error: expect.stringContaining('Ya existe'),
    });
  });

  test('Razón social vacía es rechazada (400)', async ({ apiAdmin }) => {
    const respuesta = await apiAdmin.post('/api/empresas', { razonSocial: '', nit: nuevaEmpresa().nit });
    expect(respuesta.status()).toBe(400);
  });

  test('NIT vacío es rechazado (400)', async ({ apiAdmin }) => {
    const respuesta = await apiAdmin.post('/api/empresas', { razonSocial: 'Empresa Sin NIT', nit: '' });
    expect(respuesta.status()).toBe(400);
  });
});

test.describe('Validaciones — Tarea', () => {
  test('Crear tarea con workflow inválido es rechazado por el dominio (400 con mensaje)', async ({ apiAdmin }) => {
    const respuesta = await apiAdmin.post('/api/tareas', {
      idWorkflow: 0, // inválido: la tarea debe pertenecer a un workflow válido
      asunto: 'Tarea inválida',
      responsable: 'resp',
      paso: 1,
      fechaVencimiento: new Date(Date.now() + 86_400_000).toISOString(),
    });
    expect(respuesta.status()).toBe(400);
    expect((await respuesta.json()) as { error: string }).toMatchObject({
      error: expect.stringContaining('workflow'),
    });
  });
});

test.describe('Recursos inexistentes → 404', () => {
  const idInexistente = 999_999_999;

  test('GET empresa inexistente → 404', async ({ apiAdmin }) => {
    expect((await apiAdmin.get(`/api/empresas/${idInexistente}`)).status()).toBe(404);
  });

  test('GET formulario inexistente → 404', async ({ apiAdmin }) => {
    expect((await apiAdmin.get(`/api/formularios/${idInexistente}`)).status()).toBe(404);
  });

  test('GET tarea inexistente → 404', async ({ apiAdmin }) => {
    expect((await apiAdmin.get(`/api/tareas/${idInexistente}`)).status()).toBe(404);
  });
});

test.describe('Paginación — normalización de parámetros', () => {
  test('tamano por encima del máximo se limita a 100', async ({ apiAdmin }) => {
    const respuesta = await apiAdmin.get('/api/empresas?pagina=1&tamano=1000');
    expect(respuesta.ok()).toBeTruthy();
    expect(((await respuesta.json()) as { tamanoPagina: number }).tamanoPagina).toBe(100);
  });

  test('pagina menor que 1 se normaliza a 1', async ({ apiAdmin }) => {
    const respuesta = await apiAdmin.get('/api/empresas?pagina=0&tamano=20');
    expect(respuesta.ok()).toBeTruthy();
    expect(((await respuesta.json()) as { pagina: number }).pagina).toBe(1);
  });

  test('tamano menor que 1 cae al valor por defecto (20)', async ({ apiAdmin }) => {
    const respuesta = await apiAdmin.get('/api/empresas?pagina=1&tamano=0');
    expect(respuesta.ok()).toBeTruthy();
    expect(((await respuesta.json()) as { tamanoPagina: number }).tamanoPagina).toBe(20);
  });
});
