import { test, expect } from '../fixtures/auth.fixture.js';
import { nuevaEmpresa, nuevaSucursal } from '../helpers/test-data.js';
import type { ApiClient } from '../helpers/api-client.js';

/**
 * Sucursales (CU-005 a CU-007). Las sucursales cuelgan de una empresa
 * (`sucursales.id_empresa` FK) y se gestionan bajo EmpresasController:
 *  - POST /api/empresas/{id}/sucursales  { codigo, nombre, direccion, telefono } → 201 { id }
 *  - GET  /api/empresas/{id}/sucursales                                          → SucursalDto[]
 *
 * Cada test crea su propia empresa para listar de forma determinista (la API no
 * expone borrado de empresa; usar empresa dedicada evita asserts frágiles sobre
 * datos compartidos).
 */
async function crearEmpresa(api: ApiClient): Promise<number> {
  const creacion = await api.post('/api/empresas', nuevaEmpresa());
  expect(creacion.status(), await creacion.text()).toBe(201);
  return ((await creacion.json()) as { id: number }).id;
}

test.describe('Núcleo Multi-Tenant — Sucursales', () => {
  test('CU-005 · Crear sucursal y verla en el listado de su empresa', async ({ apiAdmin }) => {
    // Arrange
    const idEmpresa = await crearEmpresa(apiAdmin);
    const datos = nuevaSucursal();

    // Act — crear
    const creacion = await apiAdmin.post(`/api/empresas/${idEmpresa}/sucursales`, datos);
    expect(creacion.status(), await creacion.text()).toBe(201);
    const { id } = (await creacion.json()) as { id: number };
    expect(id).toBeGreaterThan(0);

    // Assert — listar
    const listado = await apiAdmin.get(`/api/empresas/${idEmpresa}/sucursales`);
    expect(listado.ok()).toBeTruthy();
    const sucursales = (await listado.json()) as Array<{
      id: number;
      idEmpresa: number;
      codigo: string;
      nombre: string;
      activa: boolean;
    }>;

    expect(sucursales).toHaveLength(1);
    const creada = sucursales[0];
    expect(creada.id).toBe(id);
    expect(creada.idEmpresa).toBe(idEmpresa);
    expect(creada.codigo).toBe(datos.codigo);
    expect(creada.nombre).toBe(datos.nombre);
    expect(creada.activa).toBe(true); // nueva sucursal nace activa
  });

  test('CU-006 · Una empresa recién creada no tiene sucursales', async ({ apiAdmin }) => {
    const idEmpresa = await crearEmpresa(apiAdmin);
    const listado = await apiAdmin.get(`/api/empresas/${idEmpresa}/sucursales`);
    expect(listado.ok()).toBeTruthy();
    expect((await listado.json()) as unknown[]).toHaveLength(0);
  });

  test('CU-007 · Crear sucursal requiere autenticación de admin (401 sin token)', async ({ apiAnonimo }) => {
    const respuesta = await apiAnonimo.post('/api/empresas/1/sucursales', nuevaSucursal());
    expect(respuesta.status()).toBe(401);
  });
});
