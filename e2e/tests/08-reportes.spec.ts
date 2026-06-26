import { test, expect } from '../fixtures/auth.fixture.js';
import {
  nuevaEmpresa,
  nuevaCategoriaReporte,
  nuevoReporte,
} from '../helpers/test-data.js';
import type { ApiClient } from '../helpers/api-client.js';

/**
 * Reportes (CU-060 a CU-062). Módulo Reportes, BD por empresa (policy Gestion).
 * Rutas absolutas por acción (no hay [Route] de clase):
 *  - GET    /api/categorias-reporte?pagina&tamano       → ResultadoPaginado<CategoriaReporteDto>
 *  - POST   /api/categorias-reporte  { codigo, categoria, descripcion? } → 201 { id }
 *  - GET    /api/categorias-reporte/{idCategoria}/reportes → ReporteDto[]
 *  - POST   /api/reportes  { idCategoria, codigo, reporte, url, descripcion? } → 201 { id }
 *  - PUT    /api/reportes/{id}  { reporte, url, descripcion? }            → 204
 *  - DELETE /api/reportes/{id}                                           → 204
 *
 * CategoriaReporteDto = { id, codigo, categoria, descripcion }
 * ReporteDto          = { id, idCategoria, codigo, reporte, url, descripcion }
 *
 * Las categorías solo tienen listado paginado (sin GET-por-id): ese test usa una
 * empresa nueva (BD limpia). Los reportes se leen vía listado por categoría; como
 * cada test crea su propia categoría nueva, ese listado es determinista aun en la
 * empresa inicial.
 */
interface ReporteDto {
  id: number;
  idCategoria: number;
  codigo: string;
  reporte: string;
  url: string;
  descripcion?: string;
}

async function clienteEmpresaLimpia(apiAdmin: ApiClient): Promise<ApiClient> {
  const creacion = await apiAdmin.post('/api/empresas', nuevaEmpresa());
  expect(creacion.status(), await creacion.text()).toBe(201);
  const idEmpresa = ((await creacion.json()) as { id: number }).id;
  const aprov = await apiAdmin.post(`/api/empresas/${idEmpresa}/aprovisionar`);
  expect(aprov.status(), await aprov.text()).toBe(200);
  return apiAdmin.con({ idEmpresa });
}

async function crearCategoria(api: ApiClient): Promise<{ id: number; datos: ReturnType<typeof nuevaCategoriaReporte> }> {
  const datos = nuevaCategoriaReporte();
  const creacion = await api.post('/api/categorias-reporte', datos);
  expect(creacion.status(), await creacion.text()).toBe(201);
  const { id } = (await creacion.json()) as { id: number };
  expect(id).toBeGreaterThan(0);
  return { id, datos };
}

async function listarReportes(api: ApiClient, idCategoria: number): Promise<ReporteDto[]> {
  const listado = await api.get(`/api/categorias-reporte/${idCategoria}/reportes`);
  expect(listado.ok()).toBeTruthy();
  return (await listado.json()) as ReporteDto[];
}

test.describe('Reportes — Categorías', () => {
  test('CU-060 · Crear categoría y verla en el listado de una empresa nueva', async ({ apiAdmin }) => {
    const cliente = await clienteEmpresaLimpia(apiAdmin);
    const { datos } = await crearCategoria(cliente);

    const respuesta = await cliente.get('/api/categorias-reporte?pagina=1&tamano=100');
    expect(respuesta.ok()).toBeTruthy();
    const pagina = (await respuesta.json()) as { elementos: Array<{ codigo: string; categoria: string }>; total: number };
    expect(pagina.total).toBe(1);
    expect(pagina.elementos[0].codigo).toBe(datos.codigo);
    expect(pagina.elementos[0].categoria).toBe(datos.categoria);
  });

  test('CU-060b · Crear categoría requiere autenticación (401 sin token)', async ({ apiAnonimo }) => {
    const respuesta = await apiAnonimo.post('/api/categorias-reporte', nuevaCategoriaReporte());
    expect(respuesta.status()).toBe(401);
  });
});

test.describe('Reportes — Reportes SSRS', () => {
  test('CU-061 · Crear reporte bajo una categoría y listarlo por categoría', async ({ apiAdmin }) => {
    const { id: idCategoria } = await crearCategoria(apiAdmin);
    const datos = nuevoReporte(idCategoria);

    const creacion = await apiAdmin.post('/api/reportes', datos);
    expect(creacion.status(), await creacion.text()).toBe(201);
    const { id } = (await creacion.json()) as { id: number };

    const reportes = await listarReportes(apiAdmin, idCategoria);
    expect(reportes).toHaveLength(1);
    expect(reportes[0].id).toBe(id);
    expect(reportes[0].idCategoria).toBe(idCategoria);
    expect(reportes[0].codigo).toBe(datos.codigo);
    expect(reportes[0].url).toBe(datos.url);
  });

  test('CU-061b · Actualizar reporte se refleja en el listado por categoría', async ({ apiAdmin }) => {
    const { id: idCategoria } = await crearCategoria(apiAdmin);
    const creacion = await apiAdmin.post('/api/reportes', nuevoReporte(idCategoria));
    const { id } = (await creacion.json()) as { id: number };

    const actualizacion = await apiAdmin.put(`/api/reportes/${id}`, {
      reporte: 'Reporte Renombrado',
      url: 'https://ssrs.example.com/reporte-actualizado',
      descripcion: 'Descripción actualizada',
    });
    expect(actualizacion.status(), await actualizacion.text()).toBe(204);

    const [reporte] = await listarReportes(apiAdmin, idCategoria);
    expect(reporte.reporte).toBe('Reporte Renombrado');
    expect(reporte.url).toBe('https://ssrs.example.com/reporte-actualizado');
  });

  test('CU-061c · Eliminar reporte lo quita del listado por categoría', async ({ apiAdmin }) => {
    const { id: idCategoria } = await crearCategoria(apiAdmin);
    const creacion = await apiAdmin.post('/api/reportes', nuevoReporte(idCategoria));
    const { id } = (await creacion.json()) as { id: number };

    const eliminacion = await apiAdmin.del(`/api/reportes/${id}`);
    expect(eliminacion.status(), await eliminacion.text()).toBe(204);

    expect(await listarReportes(apiAdmin, idCategoria)).toHaveLength(0);
  });
});
