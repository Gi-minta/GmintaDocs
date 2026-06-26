# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

GmintaDocs is a multi-tenant document-management system. The repo has two independent halves:

- `backend/` — .NET 10 modular monolith (Clean Architecture, custom CQRS, multi-tenant PostgreSQL).
- `frontend/` — React 19 + Vite 8 + TypeScript SPA.

Both halves are implemented end-to-end. The ten backend modules carry rich domain aggregates (factories returning `Result`), CQRS commands/queries/handlers, EF repositories + DbContexts (snake_case), and REST controllers; the host wires JWT auth and per-tenant resolution; every module has an initial EF migration. The frontend is a working SPA (auth flow + module pages) rather than the Vite starter. New feature work means *extending* these layers, not rewiring the structure.

Module coverage: `01-Identidad` and `01-Organizacion` live in the **master/control DB**; the eight business modules (`02-AdminFormularios`, `02-AdminDirectorios`, `03-Workflow`, `03-Tareas`, `04-GestionDocumental`, `05-Plantillas`, `06-Reportes`, `07-Colaboracion`) live in **per-empresa DBs**. Each module's **primary aggregate now has full CRUD** (create/read + an `Editar(...)` domain method behind an `Actualizar…` command and an `Eliminar…` command, each with PUT/DELETE endpoints); some secondary lookup entities remain mapped-only (no CQRS yet). The domain mutation lives in the aggregate (returns `Result`); handlers load → mutate → `Actualizar`/`Eliminar` on the repo → save.

## Language convention

Domain code is written in **Spanish** (class, method, property, and interface names): `IComando`, `IManejadorDeConsulta`, `Despachador`, `Result.Exitoso()/Fallido()`, `IContextoDeTenant.CadenaDeConexion`. Match this when adding domain, application, and infrastructure code. Framework/config identifiers stay in their native English. Folders use Spanish module names (`Identidad`, `Tareas`, `GestionDocumental`, …).

## Commands

### Backend (run from `backend/`)
- Build: `dotnet build GmintaDocs.slnx`
- Run API: `dotnet run --project src/Host/GmintaDocs.Api` (http on :5289, https on :7275; OpenAPI at `/openapi` in Development)
- `dotnet ef` is pinned as a **local tool** (`.config/dotnet-tools.json`); run `dotnet tool restore` once, then invoke `dotnet ef …`.
- Add EF migration (per module Infrastructure project, always pass `--context`): `dotnet ef migrations add <Name> --project src/Modules/<NN-Module>/<...>.Infrastructure --startup-project src/Host/GmintaDocs.Api --context <XxxDbContext>`. The eight per-tenant DbContexts each ship an `IDesignTimeDbContextFactory` (subclass of `FabricaDeDisenioDeTenant<T>`) so design-time tooling works without an active tenant — override the design connection string with the `GMINTA_TENANT_CNX` env var.
- There is **no test project yet**; add one before relying on a `dotnet test` workflow.

### Frontend (run from `frontend/`)
- Dev server: `npm run dev`
- Production build (type-checks first): `npm run build` (runs `tsc -b && vite build`)
- Lint: `npm run lint`

## Backend architecture

### Module layout
Each business capability is a numbered module under `src/Modules/<NN-Nombre>/` split into three projects following Clean Architecture, with a strict reference direction:

- `*.Domain` → references only `Shared/GmintaDocs.SharedKernel`. Holds entities/aggregates.
- `*.Application` → references its own `Domain` + `Shared/GmintaDocs.CQRS`. Holds commands, queries, and their handlers.
- `*.Infrastructure` → references its own `Application` + `Shared/GmintaDocs.Multitenancy` + EF Core (`Microsoft.EntityFrameworkCore`, `Npgsql.EntityFrameworkCore.PostgreSQL`). Holds DbContexts, persistence, and DI wiring.

The `NN` prefix encodes dependency/build order (01 = foundational like `Identidad`/`Organizacion`, higher numbers build on lower ones). `GmintaDocs.Api` (the only Host) references **every module's Infrastructure project** plus the shared CQRS and Multitenancy projects — it is the composition root.

### Shared kernel (`src/Shared`)
- `GmintaDocs.SharedKernel` — `Entity<TId>` (identity-based equality), `AggregateRoot<TId>` (bounded-context root marker), `Result` / `Result<TValue>` for expected business-flow failures **without exceptions** (prefer returning `Result` over throwing for domain validation), and pagination primitives `ParametrosDePaginacion` (normalizes page/size, max 100) + `ResultadoPaginado<T>` (`Elementos`/`Pagina`/`TamanoPagina`/`Total`/`TotalPaginas`, with `.Mapear(entity→dto)`).
- `GmintaDocs.CQRS` — a hand-rolled MediatR replacement. `IComando<TRespuesta>` / `IConsulta<TRespuesta>` mark intents; `IManejadorDeComando<,>` / `IManejadorDeConsulta<,>` are the handlers; `Despachador` (`IDespachador`) resolves the handler from DI by constructed generic type and invokes it via `dynamic`. Handlers must be registered in DI for the dispatcher to find them.
- **Pagination**: the "list-all" query of each primary aggregate is paginated. The repo exposes `ListarPaginadoAsync(ParametrosDePaginacion)` → `ResultadoPaginado<TEntity>` (built by the `IQueryable.PaginarAsync` EF extension in `GmintaDocs.Multitenancy` — DB-side `Count` + `Skip/Take`); the `Listar…` query record carries `ParametrosDePaginacion` and returns `ResultadoPaginado<TDto>` via `.Mapear`; the controller binds `[FromQuery] int pagina = 1, int tamano = 20`. Filtered/child lists (by formulario, categoría, responsable, etc.) stay unpaginated. Frontend mirrors this with the `useListaPaginada` hook + `componentes/Paginacion` control.
- `GmintaDocs.Multitenancy` — `IContextoDeTenant` exposes the active tenant (`IdEmpresa`, `CadenaDeConexion`, `IdSucursal`); `ContextoDeTenant` is the mutable scoped implementation (filled per request). The model is **database-per-empresa**. `AgregarDbContextDeTenant<T>()` registers a DbContext whose connection string is resolved at request scope from `IContextoDeTenant.CadenaDeConexion`. Since the `empresas` table has no connection column, `IResolvedorDeTenant` (`ResolvedorDeTenantDesdeConfiguracion`) resolves it from config — `Tenants:{IdEmpresa}` first, else the `ConnectionStrings:PlantillaTenant` template with `{IdEmpresa}` substituted (documented deviation from the DDL). Each per-tenant DbContext has a `FabricaDeDisenioDeTenant<T>` design-time factory.

### Host wiring (`GmintaDocs.Api`)
- **Composition root** `Program.cs`: registers the `Despachador`, `AgregarMultitenancy()`, the two master modules against the `Maestro` connection string, and the eight business modules. Pipeline order matters: `UseCors()` → `UseAuthentication()` → `MiddlewareDeTenant` → `UseAuthorization()`.
- **Tenant resolution** `MiddlewareDeTenant` fills the scoped `ContextoDeTenant` before any per-empresa DbContext is touched, taking the empresa from the `X-Id-Empresa` header (admin/tooling override) or the `id_empresa` JWT claim.
- **Auth** is JWT Bearer (HMAC-SHA256, `Seguridad/`): `POST api/auth/login` validates credentials via the `AutenticarUsuario` query (passwords hashed with PBKDF2 in `HasheadorDeContrasenaPbkdf2`) and `GeneradorDeTokensJwt` issues a token carrying `id_empresa`/`id_sucursal`. Config lives in the `Jwt` section (`ClaveSecreta` must be ≥32 chars). **Note: the role claim carries the role's `RoleId`, not its name** — so well-known roles are seeded with `Id == Nombre` (see `Seguridad/RolesDelSistema.cs`) for `[Authorize(Roles = …)]` to match.
- **Authorization**: only `POST api/auth/login` is `[AllowAnonymous]` (action-level; the controller is no longer class-level anonymous). The núcleo admin controllers (Usuarios, Roles, Empresas, Aprovisionamiento) require `[Authorize(Roles = RolesDelSistema.Administrador)]`; the eight business controllers require the `Gestion` policy (`Administrador` or `Gestor`), declared in `Seguridad/PoliticasDeAutorizacion.cs` and registered via `AddAuthorization(PoliticasDeAutorizacion.Registrar)`. `POST api/auth/cambiar-contrasena` is `[Authorize]` (any authenticated user changes their own password via the `CambiarContrasena` command).
- **Errors**: `AddProblemDetails()` + a global `IExceptionHandler` (`Errores/ManejadorGlobalDeExcepciones`, wired first via `UseExceptionHandler()`) turn unhandled exceptions into RFC-7807 `ProblemDetails` (detail only shown in Development). Expected business failures still return `BadRequest(new { error })` from `Result.Fallido`.
- **OpenAPI**: `AddOpenApi(... AddDocumentTransformer<TransformadorDeSeguridadOpenApi>())` (in `Configuracion/`) declares the `Bearer` security scheme so protected endpoints are testable. In Development `MapScalarApiReference()` (package `Scalar.AspNetCore`) serves an interactive UI at `/scalar` with a Bearer auth button; the raw doc is at `/openapi/v1.json`.
- **CORS**: a single policy (`SpaGmintaDocs`) reads allowed origins from `Cors:OrigenesPermitidos` (empty array = no external origin enabled); needed only when the SPA is served outside the Vite dev proxy.
- **Seeding & bootstrap** (`Persistencia/SembradorDeIdentidad.cs`): when `Persistencia:MigrarMaestraAlArrancar` is true, after migrating the master DB the host seeds the system roles and an initial admin user (idempotent). Admin credentials come from `Seguridad:Admin` (`UserName`/`Contrasena`/…); a blank `Contrasena` skips admin seeding with a warning.
- **Migrations** (`Persistencia/`): the master DB (Identidad + Organización) auto-migrates at startup under the same flag; per-empresa DBs are created+migrated on demand by `AprovisionadorDeEmpresa` via `POST api/empresas/{id}/aprovisionar`.
- Typical bootstrap: start API (seeds admin) → `login` as admin → create empresa → `aprovisionar` its DB → create users/assign roles → call business endpoints (token carries the empresa).

### Build settings
`backend/Directory.Build.props` applies to all projects: `net10.0`, nullable enabled with **`<WarningsAsErrors>Nullable</WarningsAsErrors>`** (nullable-reference violations break the build), implicit usings, latest C#. The solution uses the newer `.slnx` format.

## Frontend architecture

Vite SPA entry at `src/main.tsx` → `src/App.tsx`. The **React Compiler is enabled** (`babel-plugin-react-compiler` via the Babel plugin in `vite.config.ts`) — avoid manual `useMemo`/`useCallback` micro-optimizations the compiler handles, and keep components following the Rules of React so it can optimize them. ESLint uses the flat-config `eslint.config.js`.

Layout and conventions:
- `App.tsx` defines the React Router 7 tree: a public `/login` and a `RutaProtegida` guard wrapping the `Layout` (sidebar shell) with pages `Panel`, `Noticias`, `Tareas`, `Usuarios` under `src/paginas/`.
- **Auth**: `ProveedorAuth` (in `auth/AuthContext.tsx`) holds the session, persisting token + empresa in `localStorage`; consume it with the `useAuth` hook. The context object/type live in `auth/contexto.ts` (kept out of the component file so React Fast Refresh / `react-refresh/only-export-components` stays happy).
- **API access**: `api/cliente.ts` is a small `fetch` wrapper exposing `api.get/post/put/del`; it attaches the `Authorization: Bearer` header and signals session loss on 401. DTO shapes live in `tipos/modelos.ts` and must mirror the backend's camelCase JSON.
- **CRUD UI**: module pages follow a consistent pattern — an inline "Editar" toggle (per-row state pre-filled from the item, Guardar/Cancelar) calling `api.put`, and a shared `componentes/BotonEliminar` (confirm-then-`api.del`). `paginas/Usuarios.tsx` additionally assigns roles (`POST /usuarios/{id}/roles`) and deactivates users (`POST /usuarios/{id}/desactivar`), pulling roles via a second `useLista<Rol>('/roles')`.
- The dev server proxies `/api` → `http://localhost:5289` (set in `vite.config.ts`); override the base URL with `VITE_API_URL`.
- Two TS-config gotchas enforced by the build: `verbatimModuleSyntax` requires `import type` for type-only imports, and `erasableSyntaxOnly` forbids constructor parameter properties and other non-erasable TS syntax. Data-loading effects must `await` before calling `setState` (the lint rule `react-hooks/set-state-in-effect` flags synchronous state updates in effects) — use an `async` IIFE with an `activo` unmount guard.

Domain-term naming (Spanish) carries over to the frontend (`iniciarSesion`, `RutaProtegida`, `paginas/`, `tipos/`), matching the backend convention.
