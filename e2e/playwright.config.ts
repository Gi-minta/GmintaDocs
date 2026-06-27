import { defineConfig } from '@playwright/test';

/**
 * Configuración E2E (a nivel de API) para GmintaDocs.
 *
 * El backend escucha en https://localhost:7275 (y http://localhost:5289, que
 * hace 307 redirect a https). Apuntamos directo a https para evitar el redirect;
 * el certificado de desarrollo es autofirmado, por eso `ignoreHTTPSErrors`.
 * Se puede sobreescribir con la variable de entorno API_BASE_URL.
 *
 * Multi-tenant: se resuelve con la cabecera `X-Id-Empresa` sobre un único
 * endpoint (decisión confirmada; ver §1.3/§11.5 del doc de referencia y el
 * MiddlewareDeTenant del backend), no con dos baseURL distintas.
 */
const API_BASE_URL = process.env.API_BASE_URL ?? 'https://localhost:7275';

export default defineConfig({
  testDir: './tests',
  // Restablece el entorno tras la suite: borra empresas de prueba y sus BD
  // por-tenant, conservando solo la empresa demo sembrada (ver el archivo).
  globalTeardown: './global-teardown.ts',
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,
  workers: 1,
  reporter: [['list'], ['html', { open: 'never' }]],
  use: {
    baseURL: API_BASE_URL,
    ignoreHTTPSErrors: true,
    extraHTTPHeaders: {
      Accept: 'application/json',
    },
    trace: 'on-first-retry',
  },
});
