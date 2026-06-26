import { test as base, expect } from '@playwright/test';
import { ApiClient } from '../helpers/api-client.js';
import { CONFIG } from '../helpers/test-data.js';

interface SesionAdmin {
  token: string;
  expiraEn: string;
  usuario: { id: string; userName?: string; fullName?: string; roles: string[] };
}

interface FixturesAuth {
  /** Cliente sin autenticar (para login y casos anónimos). */
  apiAnonimo: ApiClient;
  /** Sesión del admin sembrado (token + usuario). */
  sesionAdmin: SesionAdmin;
  /** Cliente autenticado como admin, con la empresa inicial como tenant activo. */
  apiAdmin: ApiClient;
}

export const test = base.extend<FixturesAuth>({
  apiAnonimo: async ({ request }, use) => {
    await use(new ApiClient(request));
  },

  sesionAdmin: async ({ apiAnonimo }, use) => {
    const respuesta = await apiAnonimo.post('/api/auth/login', {
      userName: CONFIG.admin.userName,
      contrasena: CONFIG.admin.contrasena,
      idEmpresa: CONFIG.empresaInicialId,
    });
    expect(
      respuesta.ok(),
      `Login admin falló (${respuesta.status()}). ¿API arriba y admin sembrado?`,
    ).toBeTruthy();
    await use((await respuesta.json()) as SesionAdmin);
  },

  apiAdmin: async ({ request, sesionAdmin }, use) => {
    await use(
      new ApiClient(request, {
        token: sesionAdmin.token,
        idEmpresa: CONFIG.empresaInicialId,
      }),
    );
  },
});

export { expect };
