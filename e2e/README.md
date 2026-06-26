# GmintaDocs — Pruebas E2E (API) con Playwright

Suite E2E a nivel de API, basada en `../GmintaDocs-Datos-Prueba-E2E.md` pero
**anclada al contrato real** de los controllers del backend (no a los supuestos
del doc de referencia).

## Requisitos previos

1. **Postgres** arriba (contenedor `gmintadocs-db`, puerto 5433):
   ```bash
   docker compose -f ../docker-compose.yml up -d
   ```
2. **API** arriba en `https://localhost:7275` (http://localhost:5289 hace 307 a
   https). Al arrancar siembra el admin + la empresa inicial (id 1):
   ```bash
   cd ../backend && dotnet run --project src/Host/GmintaDocs.Api
   ```
   El cert de desarrollo es autofirmado; la config usa `ignoreHTTPSErrors`.

## Instalar y ejecutar

```bash
npm install
npm test            # corre todos los specs
npm run report      # abre el reporte HTML
```

## Configuración (variables de entorno opcionales)

| Variable             | Defecto                  | Uso                                   |
|----------------------|--------------------------|---------------------------------------|
| `API_BASE_URL`       | `https://localhost:7275` | Base de la API (https)                |
| `ADMIN_USERNAME`     | `admin`                  | Usuario admin sembrado                |
| `ADMIN_PASSWORD`     | `admin123`               | Contraseña admin                      |
| `EMPRESA_INICIAL_ID` | `1`                      | Empresa aprovisionada al arrancar     |

## Estructura

```
e2e/
├── fixtures/   auth.fixture.ts   (login admin + cliente autenticado)
├── helpers/    api-client.ts     (wrapper con Authorization + X-Id-Empresa)
│               test-data.ts      (config + generadores de fixtures)
└── tests/      01-empresas.spec.ts                 (login → crear empresa → aprovisionar)
                02-sucursales.spec.ts               (crear sucursal + listar por empresa)
                03-identidad.spec.ts                (roles, usuarios, asignar rol, desactivar, login)
                04-multi-tenant-aislamiento.spec.ts (BD por empresa: datos no cruzan entre tenants)
                05-formularios-campos.spec.ts       (formularios CRUD + campos + directorios)
                06-gestion-documental.spec.ts       (tipos de documento + archivos/versiones)
                07-workflow-tareas.spec.ts          (procesos/pasos/workflow + tareas: crear/ejecutar/reasignar)
                08-reportes.spec.ts                 (categorías de reporte + reportes SSRS)
                09-validaciones-negativas.spec.ts   (400/404 + normalización de paginación)
                10-autorizacion.spec.ts             (matriz de roles: Gestor/Usuario vs admin/negocio)
                11-concurrencia.spec.ts             (ejecución/creación concurrente; carrera de NIT)
```

## Multi-tenant

El aislamiento se resuelve con la cabecera `X-Id-Empresa` sobre un único
endpoint (ver `MiddlewareDeTenant`), no con dos `baseURL` distintas.
`ApiClient.con({ idEmpresa })` permite alternar el tenant activo.
