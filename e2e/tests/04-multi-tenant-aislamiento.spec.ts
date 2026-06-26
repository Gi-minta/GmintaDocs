import { test, expect } from '../fixtures/auth.fixture.js';
import { CONFIG, nuevaEmpresa, nuevaNoticia } from '../helpers/test-data.js';
import type { ApiClient } from '../helpers/api-client.js';

/**
 * Aislamiento Multi-Tenant (CU-004 / RN-001).
 *
 * El modelo es **base-de-datos-por-empresa**: las entidades de negocio (aquí
 * `noticias`, módulo Colaboración) viven en la BD dedicada de cada empresa. El
 * tenant activo se selecciona con la cabecera `X-Id-Empresa`, que el
 * MiddlewareDeTenant antepone al claim `id_empresa` del JWT. Por eso el mismo
 * token de admin puede operar contra distintos tenants cambiando solo el header
 * (`apiAdmin.con({ idEmpresa })`) — NO se usan dos baseURL distintas.
 *
 * Empresa A = empresa inicial (id 1, ya aprovisionada). Empresa B se crea y
 * aprovisiona dentro de la suite para tener una BD limpia y separada.
 *
 * NoticiaDto = { id, titulo, autor, texto, fechaPublicacion }.
 */
async function crearYAprovisionarEmpresa(api: ApiClient): Promise<number> {
  const creacion = await api.post('/api/empresas', nuevaEmpresa());
  expect(creacion.status(), await creacion.text()).toBe(201);
  const { id } = (await creacion.json()) as { id: number };

  const aprov = await api.post(`/api/empresas/${id}/aprovisionar`);
  expect(aprov.status(), await aprov.text()).toBe(200);
  return id;
}

interface Noticia {
  id: number;
  titulo: string;
}

async function publicarNoticia(api: ApiClient): Promise<Noticia> {
  const datos = nuevaNoticia();
  const creacion = await api.post('/api/noticias', datos);
  expect(creacion.status(), await creacion.text()).toBe(201);
  const { id } = (await creacion.json()) as { id: number };
  return { id, titulo: datos.titulo };
}

test.describe('Aislamiento Multi-Tenant', () => {
  test('CU-004a · Una noticia creada en la empresa A no es accesible por su id desde la empresa B', async ({ apiAdmin }) => {
    // Arrange — dos tenants distintos (B con BD recién aprovisionada/vacía)
    const clienteA = apiAdmin.con({ idEmpresa: CONFIG.empresaInicialId });
    const idEmpresaB = await crearYAprovisionarEmpresa(apiAdmin);
    const clienteB = apiAdmin.con({ idEmpresa: idEmpresaB });

    // Act — crear en A
    const noticiaA = await publicarNoticia(clienteA);

    // Assert — visible en A (control), ausente en B (aislamiento)
    const enA = await clienteA.get(`/api/noticias/${noticiaA.id}`);
    expect(enA.ok()).toBeTruthy();
    expect(((await enA.json()) as Noticia).titulo).toBe(noticiaA.titulo);

    const enB = await clienteB.get(`/api/noticias/${noticiaA.id}`);
    expect(enB.status()).toBe(404); // BD de B está vacía: el id de A no existe allí
  });

  test('CU-004b · Cada tenant nunca devuelve los datos del otro para un mismo id', async ({ apiAdmin }) => {
    // Arrange
    const clienteA = apiAdmin.con({ idEmpresa: CONFIG.empresaInicialId });
    const idEmpresaB = await crearYAprovisionarEmpresa(apiAdmin);
    const clienteB = apiAdmin.con({ idEmpresa: idEmpresaB });

    // Act — crear en B
    const noticiaB = await publicarNoticia(clienteB);

    // Assert — B ve lo suyo
    const enB = await clienteB.get(`/api/noticias/${noticiaB.id}`);
    expect(enB.ok()).toBeTruthy();
    expect(((await enB.json()) as Noticia).titulo).toBe(noticiaB.titulo);

    // A, para ese mismo id, jamás devuelve la noticia de B (o 404, o su propia noticia distinta).
    const enA = await clienteA.get(`/api/noticias/${noticiaB.id}`);
    if (enA.ok()) {
      expect(((await enA.json()) as Noticia).titulo).not.toBe(noticiaB.titulo);
    } else {
      expect(enA.status()).toBe(404);
    }
  });

  test('CU-004c · Una empresa recién aprovisionada arranca sin noticias (BD propia y limpia)', async ({ apiAdmin }) => {
    const idEmpresaB = await crearYAprovisionarEmpresa(apiAdmin);
    const clienteB = apiAdmin.con({ idEmpresa: idEmpresaB });

    const listado = await clienteB.get('/api/noticias');
    expect(listado.ok()).toBeTruthy();
    const pagina = (await listado.json()) as { elementos: unknown[]; total: number };
    expect(pagina.total).toBe(0);
    expect(pagina.elementos).toHaveLength(0);
  });

  test('CU-004d · Sin empresa resuelta (sin header ni claim) no se puede operar negocio', async ({ sesionAdmin, request }) => {
    // Token válido pero SIN cabecera X-Id-Empresa y SIN claim de empresa utilizable:
    // el claim id_empresa del token de admin es 1, así que para forzar "sin tenant"
    // probamos un id de empresa inexistente vía header → su BD no existe → no 200.
    const sinTenant = new (await import('../helpers/api-client.js')).ApiClient(request, {
      token: sesionAdmin.token,
      idEmpresa: 99_999_999,
    });
    const respuesta = await sinTenant.get('/api/noticias');
    expect(respuesta.ok()).toBeFalsy(); // BD inexistente → error (no expone datos de otro tenant)
  });
});
