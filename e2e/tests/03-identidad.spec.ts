import { test, expect } from '../fixtures/auth.fixture.js';
import { CONFIG, nuevoUsuario, nuevoRol } from '../helpers/test-data.js';
import type { ApiClient } from '../helpers/api-client.js';

/**
 * Identidad — Usuarios y Roles (CU-008 a CU-012). Viven en la BD maestra y
 * requieren rol Administrador.
 *
 * Contrato real:
 *  - GET  /api/roles?pagina&tamano        → ResultadoPaginado<RolDto>   { id, nombre }
 *  - POST /api/roles      { nombre }                                    → 201 { id }
 *  - GET  /api/usuarios?pagina&tamano     → ResultadoPaginado<UsuarioDto>
 *  - GET  /api/usuarios/{id}              → UsuarioDto { id, userName, fullName, email, activo, roles[] }
 *  - POST /api/usuarios   { userName, fullName?, email?, contrasena? }  → 201 { id }
 *  - POST /api/usuarios/{id}/roles        { idRol }                     → 204
 *  - POST /api/usuarios/{id}/desactivar                                 → 204
 *
 * Los roles del sistema se siembran con Id == Nombre, por eso se puede asignar
 * el rol "Gestor" usando ese literal como idRol.
 */
interface UsuarioDto {
  id: string;
  userName: string;
  fullName?: string;
  email?: string;
  activo: boolean;
  roles: string[];
}

async function crearUsuario(api: ApiClient): Promise<{ id: string; datos: ReturnType<typeof nuevoUsuario> }> {
  const datos = nuevoUsuario();
  const creacion = await api.post('/api/usuarios', datos);
  expect(creacion.status(), await creacion.text()).toBe(201);
  const { id } = (await creacion.json()) as { id: string };
  expect(id).toBeTruthy();
  return { id, datos };
}

test.describe('Identidad — Roles', () => {
  test('CU-008 · El listado de roles incluye los roles del sistema sembrados', async ({ apiAdmin }) => {
    const respuesta = await apiAdmin.get('/api/roles?pagina=1&tamano=100');
    expect(respuesta.ok()).toBeTruthy();
    const pagina = (await respuesta.json()) as { elementos: Array<{ id: string; nombre: string }> };
    const nombres = pagina.elementos.map((r) => r.nombre);
    expect(nombres).toEqual(expect.arrayContaining(['Administrador', 'Gestor', 'Usuario']));
  });

  test('CU-009 · Crear un rol nuevo devuelve 201 con su id', async ({ apiAdmin }) => {
    const creacion = await apiAdmin.post('/api/roles', nuevoRol());
    expect(creacion.status(), await creacion.text()).toBe(201);
    const { id } = (await creacion.json()) as { id: string };
    expect(id).toBeTruthy();
  });
});

test.describe('Identidad — Usuarios', () => {
  test('CU-010 · Crear usuario y consultarlo por id (nace activo y sin roles)', async ({ apiAdmin }) => {
    const { id, datos } = await crearUsuario(apiAdmin);

    const consulta = await apiAdmin.get(`/api/usuarios/${id}`);
    expect(consulta.ok()).toBeTruthy();
    const usuario = (await consulta.json()) as UsuarioDto;
    expect(usuario.userName).toBe(datos.userName);
    expect(usuario.email).toBe(datos.email);
    expect(usuario.activo).toBe(true);
    expect(usuario.roles).toHaveLength(0);
  });

  test('CU-011 · Asignar el rol Gestor a un usuario se refleja en su consulta', async ({ apiAdmin }) => {
    const { id } = await crearUsuario(apiAdmin);

    const asignacion = await apiAdmin.post(`/api/usuarios/${id}/roles`, { idRol: 'Gestor' });
    expect(asignacion.status(), await asignacion.text()).toBe(204);

    const consulta = await apiAdmin.get(`/api/usuarios/${id}`);
    const usuario = (await consulta.json()) as UsuarioDto;
    expect(usuario.roles).toContain('Gestor');
  });

  test('CU-012 · Desactivar un usuario lo deja con activo=false', async ({ apiAdmin }) => {
    const { id } = await crearUsuario(apiAdmin);

    const desactivacion = await apiAdmin.post(`/api/usuarios/${id}/desactivar`);
    expect(desactivacion.status(), await desactivacion.text()).toBe(204);

    const consulta = await apiAdmin.get(`/api/usuarios/${id}`);
    const usuario = (await consulta.json()) as UsuarioDto;
    expect(usuario.activo).toBe(false);
  });

  test('CU-012b · Un usuario recién creado puede autenticarse con su contraseña', async ({ apiAdmin, apiAnonimo }) => {
    const { datos } = await crearUsuario(apiAdmin);

    const login = await apiAnonimo.post('/api/auth/login', {
      userName: datos.userName,
      contrasena: datos.contrasena,
      idEmpresa: CONFIG.empresaInicialId,
    });
    expect(login.status(), await login.text()).toBe(200);
    const sesion = (await login.json()) as { token: string; usuario: { userName: string } };
    expect(sesion.token).toBeTruthy();
    expect(sesion.usuario.userName).toBe(datos.userName);
  });

  test('CU-012c · El listado de usuarios requiere autenticación (401 sin token)', async ({ apiAnonimo }) => {
    const respuesta = await apiAnonimo.get('/api/usuarios');
    expect(respuesta.status()).toBe(401);
  });
});
