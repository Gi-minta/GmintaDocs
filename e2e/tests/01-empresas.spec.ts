import { test, expect } from '../fixtures/auth.fixture.js';
import { nuevaEmpresa } from '../helpers/test-data.js';

/**
 * Slice vertical de Núcleo Multi-Tenant (CU-001 a CU-003): el flujo de arranque
 * documentado en CLAUDE.md → login admin → crear empresa → aprovisionar su BD.
 *
 * Contrato real (no el supuesto del doc de referencia):
 *  - POST /api/auth/login            { userName, contrasena, idEmpresa } → { token, expiraEn, usuario }
 *  - POST /api/empresas              { razonSocial, nit }                → 201 { id }
 *  - GET  /api/empresas/{id}                                            → EmpresaDto
 *  - POST /api/empresas/{id}/aprovisionar                               → 200 { idEmpresa, estado }
 */
test.describe('Núcleo Multi-Tenant — Empresas', () => {
  test('CU-001 · El admin sembrado puede autenticarse y recibe un token', async ({ sesionAdmin }) => {
    expect(sesionAdmin.token).toBeTruthy();
    expect(sesionAdmin.usuario.userName).toBe('admin');
    expect(sesionAdmin.usuario.roles).toContain('Administrador');
  });

  test('CU-001b · Login con credenciales inválidas devuelve 401', async ({ apiAnonimo }) => {
    const respuesta = await apiAnonimo.post('/api/auth/login', {
      userName: 'admin',
      contrasena: 'clave-incorrecta',
      idEmpresa: 1,
    });
    expect(respuesta.status()).toBe(401);
  });

  test('CU-001c · Login sin empresa (IdEmpresa<=0) devuelve 400', async ({ apiAnonimo }) => {
    const respuesta = await apiAnonimo.post('/api/auth/login', {
      userName: 'admin',
      contrasena: 'admin123',
      idEmpresa: 0,
    });
    expect(respuesta.status()).toBe(400);
  });

  test('CU-002/003 · Crear empresa, consultarla y aprovisionar su base de datos', async ({ apiAdmin }) => {
    // Arrange
    const datos = nuevaEmpresa();

    // Act — crear
    const creacion = await apiAdmin.post('/api/empresas', datos);
    expect(creacion.status(), await creacion.text()).toBe(201);
    const { id } = (await creacion.json()) as { id: number };
    expect(id).toBeGreaterThan(0);

    // Assert — consultar
    const consulta = await apiAdmin.get(`/api/empresas/${id}`);
    expect(consulta.ok()).toBeTruthy();
    const empresa = (await consulta.json()) as { razonSocial: string; nit: string };
    expect(empresa.razonSocial).toBe(datos.razonSocial);
    expect(empresa.nit).toBe(datos.nit);

    // Act/Assert — aprovisionar la BD dedicada del tenant
    const aprov = await apiAdmin.post(`/api/empresas/${id}/aprovisionar`);
    expect(aprov.status(), await aprov.text()).toBe(200);
    const estado = (await aprov.json()) as { idEmpresa: number; estado: string };
    expect(estado.idEmpresa).toBe(id);
    expect(estado.estado).toBe('aprovisionada');
  });

  test('CU-002b · Crear empresa requiere autenticación de admin (401 sin token)', async ({ apiAnonimo }) => {
    const respuesta = await apiAnonimo.post('/api/empresas', nuevaEmpresa());
    expect(respuesta.status()).toBe(401);
  });
});
