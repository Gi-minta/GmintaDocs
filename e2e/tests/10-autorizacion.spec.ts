import { test, expect } from '../fixtures/auth.fixture.js';
import { ApiClient } from '../helpers/api-client.js';
import { CONFIG, nuevoUsuario } from '../helpers/test-data.js';

/**
 * Matriz de autorización por rol.
 *
 *  - Endpoints de núcleo admin (Usuarios/Roles/Empresas): requieren rol Administrador.
 *  - Endpoints de negocio (Formularios, etc.): policy "Gestion" = Administrador o Gestor.
 *
 * Resultado esperado:
 *  | rol           | GET /api/usuarios (admin) | GET /api/formularios (negocio) |
 *  |---------------|---------------------------|--------------------------------|
 *  | Gestor        | 403                       | 200                            |
 *  | Usuario       | 403                       | 403                            |
 *  | (sin token)   | 401                       | 401                            |
 *
 * Los roles del sistema se siembran con Id == Nombre, así que se asignan por literal.
 */
async function crearUsuarioConRol(
  apiAdmin: ApiClient,
  apiAnonimo: ApiClient,
  request: import('@playwright/test').APIRequestContext,
  idRol: string,
): Promise<ApiClient> {
  const datos = nuevoUsuario();
  const creacion = await apiAdmin.post('/api/usuarios', datos);
  expect(creacion.status(), await creacion.text()).toBe(201);
  const { id } = (await creacion.json()) as { id: string };

  const asignacion = await apiAdmin.post(`/api/usuarios/${id}/roles`, { idRol });
  expect(asignacion.status(), await asignacion.text()).toBe(204);

  const login = await apiAnonimo.post('/api/auth/login', {
    userName: datos.userName,
    contrasena: datos.contrasena,
    idEmpresa: CONFIG.empresaInicialId,
  });
  expect(login.status(), await login.text()).toBe(200);
  const { token } = (await login.json()) as { token: string };

  return new ApiClient(request, { token, idEmpresa: CONFIG.empresaInicialId });
}

test.describe('Autorización por rol', () => {
  test('Un Gestor accede a negocio pero NO al núcleo admin', async ({ apiAdmin, apiAnonimo, request }) => {
    const gestor = await crearUsuarioConRol(apiAdmin, apiAnonimo, request, 'Gestor');

    expect((await gestor.get('/api/formularios')).status()).toBe(200); // negocio: permitido
    expect((await gestor.get('/api/usuarios')).status()).toBe(403); // admin: prohibido
  });

  test('Un Usuario no accede ni a negocio ni al núcleo admin', async ({ apiAdmin, apiAnonimo, request }) => {
    const usuario = await crearUsuarioConRol(apiAdmin, apiAnonimo, request, 'Usuario');

    expect((await usuario.get('/api/formularios')).status()).toBe(403); // negocio: prohibido
    expect((await usuario.get('/api/usuarios')).status()).toBe(403); // admin: prohibido
  });

  test('Sin token, ambos endpoints responden 401', async ({ apiAnonimo }) => {
    expect((await apiAnonimo.get('/api/formularios')).status()).toBe(401);
    expect((await apiAnonimo.get('/api/usuarios')).status()).toBe(401);
  });
});
