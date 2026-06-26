# GmintaDocs — Datos y Esquema de Referencia para Pruebas E2E (Playwright)

> Este documento complementa `playwrigth.md`. Su contenido está construido **directamente a partir de `GmintaDocs_Unificado.sql`** (DDL real), no solo del análisis funcional previo. Donde el documento de análisis (`GmintaDocs_-_Suite_de_Pruebas_E2E_con_Playwright.docx`) asumía campos o reglas que **no existen en el DDL**, se señala explícitamente más abajo.

---

## 0. Objetivo

Servir como fuente única de verdad para:
- Construir **fixtures/seed data** consistentes con las columnas reales de la BD.
- Definir el **orden de creación/limpieza** según las foreign keys reales.
- Completar los módulos de prueba que el análisis previo dejó incompletos (Workflow avanzado, Agenda, Noticias, Comunicaciones, Reportes, Listas).
- Documentar discrepancias entre el análisis funcional y el DDL real, para que las pruebas no asuman campos que no existen.

---

## 1. ⚠️ Discrepancias detectadas (análisis previo vs. SQL real)

| # | Hallazgo | Detalle |
|---|----------|---------|
| 1 | **`empresas` NO tiene columna `activo`** | El DDL de `empresas` no incluye ningún campo booleano de estado. El análisis previo (CU-004 "Desactivar empresa") asume `activo: true/false`, lo cual **no es soportado a nivel de BD**. Si la API expone un "desactivar empresa", probablemente lo hace por otra vía (tabla relacionada, soft-delete en otra capa) — **validar con backend antes de escribir el test**, o ajustar CU-004 a "no aplica a nivel de datos". |
| 2 | **`tipo_documento` NO tiene columna de estado/activo** | No hay `estado` ni `activo`. CU-031 no puede filtrar por activo a nivel de BD. |
| 3 | **`users` no tiene `id_empresa`** | Coherente con RN-001 ("cada empresa tiene base de datos separada"): el multi-tenant no se resuelve filtrando por `id_empresa` dentro de una tabla compartida de usuarios, sino por **conexión/instancia separada por tenant**. La prueba `04-multi-tenant-aislamiento.spec.ts` debe apuntar a **dos `API_BASE_URL`/conexiones distintas** (una por tenant), no a un filtro `idEmpresa` sobre el mismo dataset. |
| 4 | **No hay constraints `UNIQUE` explícitos en el DDL de negocio** | Campos como `empresas.nit`, `formularios.codigo`, `sucursales.codigo`, `directorios.codigo`, `tipo_documento.codigo` **no tienen `UNIQUE` a nivel de tabla**. La unicidad (RN-005, RN-010, etc.) se infiere como **regla de aplicación**, no de BD. Las pruebas de "rechazar duplicado" deben validar el comportamiento de la API, no asumir un error de constraint de Postgres. |
| 5 | **Campos de "estado" son códigos numéricos, no booleanos** | `formularios.estado` (SMALLINT), `directorios.id_estado` (INTEGER), `archivos.estado` (SMALLINT), `proceso.estado` (SMALLINT), `workflow.estado` (SMALLINT), `tareas.estado` (SMALLINT), `campos.estado` (SMALLINT) son códigos catalogados. Los valores `1 = activo`, `0/2 = inactivo` usados en el análisis previo son **suposiciones razonables pero no confirmadas** — confirmar tabla de catálogo real (`parametros`?) antes de fijarlos en assertions. |
| 6 | **El módulo Workflow/Agenda/Noticias/Reportes quedó sin pruebas en el documento previo** | El `.docx` se corta abruptamente dentro de CU-032 (TRD) y nunca desarrolla 4.5–4.10 (Workflow avanzado, Agenda, Noticias, Comunicaciones, Reportes, Auditoría) ni las secciones 5–7 (Fixtures completos, Matriz de Cobertura, Configuración). Este documento los cubre en las secciones 6–9. |
| 7 | **Tablas existentes en el SQL y ausentes del análisis previo** | `lista`, `item_lista`, `grupos_wf`, `miembros_grupo_wf`, `configuracion_paso`, `evidencias`, `carpeta_tarea`, `contenido_carpeta`, `plantillas`, `plantillas_formato`, `mensajes_notificacion`, `parametros`, `parametros_formato`, `parametros_mensaje`, `radicados_empresa`, `radicados_sucursal`, `relacion_archivos`, `copias`, `ayuda_almacenar`, `roles_empresa`, `online`, `opciones_rol`, `busqueda_archivos`, `posibles_pasos_devolucion`, `feriados`, `formulario_workflow`. Se documentan en la sección 5 (referencia ligera).

---

## 2. Inventario completo de tablas (55) agrupadas por módulo

### 2.1 Identidad (Identity / ASP.NET Core Identity-style)
| Tabla | PK | Notas |
|---|---|---|
| `users` | `id` (varchar 128) | `discriminator`, `code`, `full_name`, `email`, `activo` (bool) |
| `roles` | `id` | `name` |
| `user_roles` | (`user_id`,`role_id`) | N:M usuarios↔roles |
| `user_claims` | `id` (serial) | `claim_type`, `claim_value` |
| `user_logins` | (`user_id`,`login_provider`,`provider_key`) | logins externos |

### 2.2 Núcleo Multi-Tenant
| Tabla | PK | FK | Notas |
|---|---|---|---|
| `empresas` | `id_empresa` | — | **sin columna activo** (ver §1.1) |
| `sucursales` | `id_sucursal` | `id_empresa → empresas` | tiene `activa` (bool) ✔ |
| `roles_empresa` | (`id_empresa`,`id_rol`) | `id_empresa → empresas` | roles habilitados por empresa |
| `radicados_empresa` | `id` | `id_empresa → empresas` | secuencia de radicados por empresa |
| `radicados_sucursal` | `id` | `id_sucursal → sucursales` | secuencia de radicados por sucursal |
| `opciones_rol` | `id` | — | `rol` (varchar), `id_opcion`, `valor` |
| `online` | `id` (uuid) | — | sesiones activas (`usuario`, `id_sucursal`) |

### 2.3 Formularios y Catálogos
| Tabla | PK | FK | Notas |
|---|---|---|---|
| `formularios` | `id_formulario` | — | `estado` SMALLINT, `id_padre` BIGINT |
| `campos` | `id_campo` | `id_formulario → formularios` | `tipo_dato`, `control` SMALLINT (catálogos), soporta cascada (`cascada_de`, `campo1`, `campo2`) |
| `lista` | `id_lista` | — | listas de valores (jerárquicas vía `id_lista_padre`) |
| `item_lista` | `id_item` | `id_lista → lista` | ítems de lista, jerárquicos vía `id_item_padre`, `activo` (bool) |
| `copias` | `id` | `id_formulario → formularios` | relación formulario↔directorio para copias |
| `ayuda_almacenar` | `id` | `id_tipo_documento → tipo_documento` | reglas de almacenamiento sugerido |
| `mensajes_notificacion` | `id` | — | plantillas de mensaje por formulario |
| `parametros_mensaje` | `id_parametro` | `id_plantilla → mensajes_notificacion` | parámetros dinámicos del mensaje |
| `plantillas` | `id` | — | plantillas genéricas |
| `plantillas_formato` | `id` | — | plantillas HTML por formulario |
| `parametros_formato` | `id_parametro` | `id_plantilla → plantillas_formato` | parámetros dinámicos del formato |

### 2.4 Directorios y Gestión Documental
| Tabla | PK | FK | Notas |
|---|---|---|---|
| `directorios` | `id_directorio` | `id_formulario → formularios` | jerárquico vía `id_directorio_padre`, `id_estado` INTEGER |
| `archivos` | `id_archivo` | `id_tipo_documento → tipo_documento` | control de versión (`id_primera_version`, `id_archivo_principal`, `es_version_actual`) |
| `archivos_formulario` | `id` | `id_archivo → archivos`, `id_directorio → directorios` | vincula archivo a un registro de formulario/workflow |
| `tipo_documento` | `id_tipo_documento` | — | **sin columna estado/activo** (ver §1.2) |
| `trd` | `id` | `id_tipo_documental → tipo_documento` | Tabla de Retención Documental (muchísimos flags booleanos) |
| `relacion_archivos` | `id` | `id_archivo → archivos` | archivos relacionados entre sí |
| `busqueda_archivos` | (compuesta, ver SQL) | — | vista/tabla materializada de búsqueda, sin PK simple |
| `formulario_workflow` | `id` | `id_workflow → workflow` | vincula formulario/registro a una instancia de workflow |

### 2.5 Workflow
| Tabla | PK | FK | Notas |
|---|---|---|---|
| `proceso` | `id_proceso` | — | `id_formulario`, `estado`, `version` |
| `paso` | `id_paso` | `id_proceso → proceso` | `prioridad`, `plazo`, `unidad_plazo` |
| `configuracion_paso` | `id` | `id_paso → paso` | pares opción/valor por paso |
| `workflow` | `id_workflow` | `id_proceso → proceso` | instancia de proceso sobre un registro |
| `tareas` | `id_tarea` | `id_workflow → workflow` | `paso`, `responsable`, `prioridad`, `paso_destino`, `paso_siguiente` |
| `lote_wf` | `id_lote` | `id_paso → paso` | agrupación de tareas para ejecución masiva |
| `tareas_lote` | `id` | `id_lote → lote_wf`, `id_tarea → tareas` | N:M tareas↔lote |
| `grupos_wf` | `id_grupo` | — | grupos de responsables |
| `miembros_grupo_wf` | `id` | `id_grupo → grupos_wf` | miembros del grupo |
| `evidencias` | `id` | — | evidencias de ejecución de tarea (`id_workflow`, `id_tarea`) |
| `carpeta_tarea` | `id` | — | carpetas para organizar tareas, jerárquica vía `padre` |
| `contenido_carpeta` | `id` | `id_carpeta_tarea → carpeta_tarea`, `id_tarea → tareas` | contenido de la carpeta |
| `posibles_pasos_devolucion` | (`paso`,`descripcion`,`id_proceso`) | — | catálogo de pasos válidos para devolución (RN-019) |
| `feriados` | `id` | — | calendario de feriados (afecta cálculo de plazos) |

### 2.6 Agenda y Comunicaciones
| Tabla | PK | FK | Notas |
|---|---|---|---|
| `agenda` | `id` | — | soporta recurrencia (`regla_recurrencia`, `excepcion_recurrencia`) |
| `noticias` | `id_noticia` | — | `titulo`, `texto` |
| `comentarios` | `id` | `id_noticia → noticias` | comentarios sobre noticias |
| `comentarios_tarea` | `id` | `id_tarea → tareas` | comentarios sobre tareas de workflow |
| `notificaciones` | `id` | — | correo saliente (`remitente`, `para`, `cuerpo`, `enviado`) |

### 2.7 Reportes
| Tabla | PK | FK | Notas |
|---|---|---|---|
| `categoria_reportes` | `id_categoria` | — | agrupador de reportes |
| `reportes_ssrs` | `id_reporte` | `id_categoria → categoria_reportes` | `url` del reporte SSRS |

### 2.8 Auditoría
| Tabla | PK | FK | Notas |
|---|---|---|---|
| `eventos` | `id` | — | auditoría genérica (`tabla`, `id_registro` BIGINT, `accion`) |
| `eventos_identidad` | `id` | — | auditoría de identidad (`id_registro` VARCHAR 128, porque el PK de `users` es varchar) |

### 2.9 Parámetros generales
| Tabla | PK | Notas |
|---|---|---|
| `parametros` | `id_parametro` | catálogo clave/valor agrupado (`grupo`) — candidato para resolver los códigos de "estado" mencionados en §1.5 |

---

## 3. Relaciones (Foreign Keys reales del DDL)

```
users (1) ──< (N) user_claims
users (1) ──< (N) user_logins
users (N) ──< (N) roles            [vía user_roles]

empresas (1) ──< (N) sucursales
empresas (1) ──< (N) roles_empresa
empresas (1) ──< (N) radicados_empresa
sucursales (1) ──< (N) radicados_sucursal

formularios (1) ──< (N) campos
formularios (1) ──< (N) directorios
formularios (1) ──< (N) copias

directorios (1) ──< (N) archivos_formulario
archivos (1) ──< (N) archivos_formulario
archivos (1) ──< (N) relacion_archivos
tipo_documento (1) ──< (N) archivos
tipo_documento (1) ──< (N) ayuda_almacenar
tipo_documento (1) ──< (N) trd

proceso (1) ──< (N) paso
proceso (1) ──< (N) workflow
paso (1) ──< (N) configuracion_paso
paso (1) ──< (N) lote_wf
workflow (1) ──< (N) tareas
workflow (1) ──< (N) formulario_workflow
lote_wf (N) ──< (N) tareas         [vía tareas_lote]
grupos_wf (1) ──< (N) miembros_grupo_wf
tareas (1) ──< (N) comentarios_tarea
carpeta_tarea (1) ──< (N) contenido_carpeta
tareas (1) ──< (N) contenido_carpeta

noticias (1) ──< (N) comentarios

categoria_reportes (1) ──< (N) reportes_ssrs

plantillas_formato (1) ──< (N) parametros_formato
mensajes_notificacion (1) ──< (N) parametros_mensaje

lista (1) ──< (N) item_lista
```

> Nota: `campos.id_formulario`, `directorios.id_formulario`, `tareas.id_workflow`, etc. tienen FK con `ON DELETE CASCADE`. Esto es relevante para el **cleanup de fixtures**: borrar la entidad padre (p. ej. `formulario` o `workflow`) limpia automáticamente sus hijas directas — no es necesario borrar manualmente `campos`, `directorios`, `tareas`, etc., uno por uno, si se borra el padre al final del test.

---

## 4. Orden de creación recomendado para seeds (respeta FKs)

1. `empresas` → `sucursales` → `roles_empresa`
2. `users` / `roles` / `user_roles` (independiente de empresas, según §1.3)
3. `tipo_documento`
4. `formularios` → `campos` → `directorios`
5. `lista` → `item_lista`
6. `proceso` → `paso` → `configuracion_paso`
7. `workflow` (requiere `proceso`) → `tareas` (requiere `workflow`) → `lote_wf`/`tareas_lote`
8. `archivos` (requiere `tipo_documento`) → `archivos_formulario` (requiere `archivos` + `directorios`)
9. `trd` (requiere `tipo_documento`)
10. Resto de módulos (`agenda`, `noticias`, `categoria_reportes`, `reportes_ssrs`, `grupos_wf`) son independientes y pueden crearse en cualquier momento.

El orden inverso (hijo→padre) aplica para el cleanup manual; pero gracias al `ON DELETE CASCADE` (sección 3), normalmente basta borrar las entidades raíz (`empresas`, `formularios`, `proceso`/`workflow`, `tipo_documento`, `noticias`, `categoria_reportes`, `grupos_wf`, `carpeta_tarea`, `plantillas_formato`, `mensajes_notificacion`, `lista`).

---

## 5. Datos semilla — Multi-Tenant (EMP001 / EMP002)

Dado el hallazgo del §1.3 (cada tenant = BD/instancia separada), el seed multi-tenant se modela como **dos configuraciones de entorno**, no como dos filas con distinto `idEmpresa` en la misma BD:

```ts
// helpers/test-data.ts
export const TENANTS = {
  EMP001: {
    baseURL: process.env.TENANT1_BASE_URL ?? process.env.API_BASE_URL,
    empresa: {
      razonSocial: 'Empresa Tenant Uno S.A.S.',
      nit: '900111000-1',
      direccion: 'Calle 10 # 20-30',
      ciudad: 'Bogotá',
      email: 'contacto@tenant1.test',
      telefono: '6011234567',
    },
    adminUser: { email: 'admin@tenant1.test', password: 'Tenant1Admin!2026' },
  },
  EMP002: {
    baseURL: process.env.TENANT2_BASE_URL,
    empresa: {
      razonSocial: 'Empresa Tenant Dos S.A.S.',
      nit: '900222000-2',
      direccion: 'Av. Principal # 45-60',
      ciudad: 'Medellín',
      email: 'contacto@tenant2.test',
      telefono: '6047654321',
    },
    adminUser: { email: 'admin@tenant2.test', password: 'Tenant2Admin!2026' },
  },
} as const;
```

**Prueba de aislamiento (`04-multi-tenant-aislamiento.spec.ts`)** — patrón sugerido:
```ts
test('Un formulario creado en EMP001 no debe ser visible desde EMP002', async ({}) => {
  const clientA = new ApiClient(TENANTS.EMP001.baseURL);
  const clientB = new ApiClient(TENANTS.EMP002.baseURL);

  const formulario = await clientA.post('/api/formularios', { /* payload */ });
  const respuesta = await clientB.get(`/api/formularios/${formulario.idFormulario}`);

  expect(respuesta.error ?? respuesta.status).toBeTruthy(); // 404 / no encontrado
});
```

---

## 6. Fixtures de datos de prueba por entidad

> Nombres de campo en **camelCase**, asumiendo que la API serializa `snake_case → camelCase` (convención típica .NET/Postgres). **Validar contra el contrato real de la API (Swagger/OpenAPI) antes de fijar estos nombres en los tests.**

### 6.1 Empresa
```ts
{
  razonSocial: `Empresa Test ${Date.now()}`,
  nit: `900${Date.now().toString().slice(0, 6)}-1`,
  direccion: 'Calle Test 123',
  ciudad: 'Bogotá',
  url: 'https://empresa-test.example.com',
  email: `test${Date.now()}@empresa.com`,
  telefono: '1234567',
  notas: 'Empresa de prueba E2E',
  // NO incluir "activo": el DDL no tiene esa columna (ver §1.1)
}
```

### 6.2 Sucursal
```ts
{
  idEmpresa: testEmpresaId,
  codigo: `SUC${Date.now().toString().slice(0, 6)}`,
  nombre: `Sucursal Test ${Date.now()}`,
  direccion: 'Calle Sucursal 123',
  telefono: '7654321',
  activa: true, // sí existe en el DDL
}
```

### 6.3 Usuario (Identity)
```ts
{
  userName: `usuario.test.${Date.now()}`,
  fullName: 'Usuario De Prueba E2E',
  email: `usuario${Date.now()}@gmintadocs.test`,
  code: `U${Date.now().toString().slice(0, 8)}`,
  activo: true,
  birthDay: '1990-01-01',
}
```

### 6.4 Formulario
```ts
{
  codigo: `F${Date.now().toString().slice(0, 8)}`,
  tabla: `tabla_test_${Date.now().toString().slice(0, 6)}`,
  nombre: `Formulario Test ${Date.now()}`,
  descripcion: 'Formulario de prueba E2E',
  idPadre: 0,
  estado: 1, // código catalogado — confirmar valores reales
  imagen: false,
  longRadicado: 10,
}
```

### 6.5 Campo
```ts
{
  idFormulario: testFormularioId,
  orden: 1,
  nombre: 'Campo de texto de prueba',
  columna: 'campo_test',
  tipoDato: 1,   // catálogo de tipos de dato — confirmar valores
  longDato: 100,
  control: 1,    // catálogo de controles UI — confirmar valores
  estado: 1,
  unico: false,
  mostrar: true,
  mascara: null,
  requerido: true,
  sticker: false,
}
```

### 6.6 Directorio
```ts
{
  idFormulario: testFormularioId,
  idDirectorioPadre: 0,
  codigo: `DIR${Date.now().toString().slice(0, 8)}`,
  nombre: `Directorio Test ${Date.now()}`,
  idEstado: 1,
}
```

### 6.7 Tipo de Documento
```ts
{
  codigo: `TD${Date.now().toString().slice(0, 8)}`,
  nombre: `Tipo Doc Test ${Date.now()}`,
  controlaVersion: true,
  diasVigencia: 365,
  controlaVigencia: true,
  // sin campo de estado/activo (ver §1.2)
}
```

### 6.8 Archivo
```ts
{
  archivo: 'test-file',
  extension: 'pdf',
  descripcion: 'Archivo de prueba E2E',
  fechaDocumento: new Date().toISOString(),
  fechaPublicacion: new Date().toISOString(),
  directorio: 'test',
  estado: 1,
  idPrimeraVersion: 0, // se autoasigna en la 1ra versión
  esVersionActual: true,
  version: '1',
  idArchivoPrincipal: 0,
  idTipoDocumento: testTipoDocumentoId,
  bytes: 102400,
  etiquetas: 'prueba,e2e',
}
```

### 6.9 TRD (Tabla de Retención Documental)
```ts
{
  idTipoDocumental: testTipoDocumentoId,
  idFormulario: testFormularioId,
  idRegistro: 1,
  retencionAg: 5,
  retencionAc: 3,
  conservacionTotal: true,
  eliminacion: false,
  microfilmacion: false,
  seleccion: false,
  porcentajeSeleccion: 0,
  facilitativo: false, facultativo: false, sustantivo: true,
  legal: true, fiscal: false, contable: false, funcional: true,
  administrativo: false, historico: false, cientifico: false,
  cultural: false, misional: true,
  procedimiento: 'Procedimiento de prueba',
  normatividad: 'Normatividad aplicable',
  observaciones: 'Observaciones de prueba',
  ley1581: true,
  retencionElectronica: 5,
  eliminacionElectronica: 'Eliminar después de 5 años',
  fisico: true, inmaterializado: false, desmaterializado: false,
  simple: true, integro: true, autentico: true,
  firmaDigital: true, firmaBiometrica: false, estampadoCronologico: true,
  seguridad: 'Media', nivelSeguridad: 'ALTO', perteneceSgc: true,
  evidenciaNiif: false,
}
```

### 6.10 Proceso
```ts
{
  proceso: `Proceso Test ${Date.now()}`,
  descripcion: 'Proceso de prueba E2E',
  idFormulario: testFormularioId,
  estado: 1,
  version: '1.0',
}
```

### 6.11 Paso
```ts
{
  idProceso: testProcesoId,
  paso: 1,
  descripcion: 'Paso de revisión inicial',
  prioridad: 'ALTA', // catálogo: ALTA/MEDIA/BAJA según análisis previo — confirmar
  plazo: 2,
  unidadPlazo: 'DIAS',
}
```

### 6.12 Workflow
```ts
{
  idProceso: testProcesoId,
  idFormulario: testFormularioId,
  idRegistro: 1,
  estado: 1,
}
```

### 6.13 Tarea
```ts
{
  idWorkflow: testWorkflowId,
  asunto: `Tarea Test ${Date.now()}`,
  descripcion: 'Descripción de tarea de prueba',
  estado: 1,
  prioridad: 'ALTA',
  paso: 1,
  responsable: 'usuario_test',
  remitente: 'usuario_test',
  tipo: 1,
  aviso: false,
  fechaAviso: new Date().toISOString(),
  fechaRecepcion: new Date().toISOString(),
  fechaVencimiento: new Date(Date.now() + 86_400_000).toISOString(),
  dias: 1, horas: 0, minutos: 0,
  pasoDestino: 2,
  pasoSiguiente: 2,
  responsablePasoSiguiente: 'usuario_test_2',
}
```

### 6.14 Lote de Workflow + relación
```ts
// lote_wf
{ idPaso: testPasoId, estado: 1, responsable: 'usuario_test', nombre: `Lote Test ${Date.now()}` }
// tareas_lote (vincula)
{ idLote: testLoteId, idTarea: testTareaId }
```

### 6.15 Grupo de Workflow + Miembro
```ts
// grupos_wf
{ nombre: `Grupo Test ${Date.now()}`, descripcion: 'Grupo de aprobadores de prueba' }
// miembros_grupo_wf
{ idGrupo: testGrupoId, miembro: 'usuario_test' }
```

### 6.16 Configuración de Paso
```ts
{ idPaso: testPasoId, idOpcion: 1, valor: 'true' }
```

### 6.17 Evidencia
```ts
{
  idOpcion: 1,
  idWorkflow: testWorkflowId,
  idTarea: testTareaId,
  parametros: JSON.stringify({ campo: 'valor' }),
  estado: 'PENDIENTE',
  procesado: false,
  nombreEvidencia: 'Evidencia de prueba',
}
```

### 6.18 Agenda
```ts
{
  titulo: `Evento Test ${Date.now()}`,
  descripcion: 'Evento de prueba E2E',
  comienza: new Date().toISOString(),
  finaliza: new Date(Date.now() + 3_600_000).toISOString(),
  esTodoDia: false,
}
```

### 6.19 Noticia + Comentario
```ts
// noticias
{ titulo: `Noticia Test ${Date.now()}`, texto: 'Contenido de la noticia de prueba' }
// comentarios
{ idNoticia: testNoticiaId, autor: 'Usuario Test', avatar: '', texto: 'Comentario de prueba' }
```

### 6.20 Notificación
```ts
{
  remitente: 'noreply@gmintadocs.test',
  para: 'destino@gmintadocs.test',
  asunto: 'Notificación de prueba E2E',
  esHtml: false,
  cuerpo: 'Cuerpo del mensaje de prueba',
  enviado: false,
}
```

### 6.21 Categoría de Reporte + Reporte SSRS
```ts
// categoria_reportes
{ codigo: `CAT${Date.now().toString().slice(0, 6)}`, categoria: 'Categoría Test', descripcion: 'Categoría de prueba' }
// reportes_ssrs
{ idCategoria: testCategoriaId, codigo: `REP${Date.now().toString().slice(0, 6)}`, reporte: 'Reporte Test', url: 'https://ssrs.example.com/reporte-test' }
```

### 6.22 Lista + Ítem de Lista
```ts
// lista
{ nombre: 'Lista de prueba', idListaPadre: 0 }
// item_lista
{ idLista: testListaId, codigo: 'ITM01', nombre: 'Ítem de prueba', idItemPadre: 0, activo: true }
```

---

## 7. Catálogo completo de Casos de Uso (CU) — incluye módulos faltantes en el análisis previo

| Módulo | CUs | Estado en doc. previo |
|---|---|---|
| Núcleo Multi-Tenant | CU-001 a CU-012 | ✅ cubierto (con corrección de §1.1) |
| Formularios | CU-013 a CU-019 | ✅ cubierto |
| Directorios | CU-020 a CU-024 | ✅ cubierto |
| Gestión Documental | CU-025 a CU-032 | ✅ cubierto (con corrección de §1.2) |
| **Workflow avanzado** | CU-033 a CU-048 | ❌ **faltaba** — ver 7.1 |
| **Agenda** | CU-049 a CU-052 | ❌ **faltaba** — ver 7.2 |
| **Noticias** | CU-053 a CU-057 | ❌ **faltaba** — ver 7.2 |
| **Comunicaciones** | CU-058 a CU-059 | ❌ **faltaba** — ver 7.2 |
| **Reportes** | CU-060 a CU-062 | ❌ **faltaba** — ver 7.3 |
| **Auditoría** | CU-063 a CU-064 | ❌ **faltaba** — ver 7.3 |
| **Listas/Catálogos** (no numerado en análisis previo) | CU-065, CU-066 | ❌ no existía — agregado |

### 7.1 Workflow avanzado (`07-workflow-tareas.spec.ts`, `08-concurrencia-workflow.spec.ts`)
- **CU-033** Crear proceso → POST `/api/proceso`
- **CU-034** Actualizar proceso
- **CU-035** Consultar procesos
- **CU-036** Desactivar proceso (vía `estado`)
- **CU-037** Crear paso → POST `/api/paso` (requiere `idProceso`)
- **CU-038** Configurar paso → POST `/api/configuracion-paso`
- **CU-039** Iniciar workflow → POST `/api/workflow/iniciar` (requiere `idProceso`, `idFormulario`, `idRegistro`)
- **CU-040** Consultar workflow (por id, por formulario+registro)
- **CU-041** Asignar tarea → tarea creada con `responsable` definido
- **CU-042** Ejecutar tarea → cambia `estado`, registra `fechaEjecucion`, avanza `pasoSiguiente`
- **CU-043** Reasignar tarea → cambia `responsable` sin alterar `paso`
- **CU-044** Consultar tareas asignadas → filtro por `responsable`
- **CU-045** Finalizar workflow → cuando la última tarea se ejecuta, `workflow.fechaFinalizacion` se establece
- **CU-046** Devolver tarea → usa `posibles_pasos_devolucion` para validar que el paso destino es válido (RN-019)
- **CU-047** Crear lote de tareas → POST `/api/lote-wf` + vincular vía `tareas_lote`
- **CU-048** Ejecutar lote → ejecuta todas las tareas vinculadas al lote en una sola operación

**Caso de concurrencia (CU-042/043) sugerido:**
```ts
test('No debería permitir que dos usuarios ejecuten la misma tarea simultáneamente', async ({ authenticatedApiClient, testTareaId }) => {
  const [r1, r2] = await Promise.all([
    authenticatedApiClient.patch(`/api/tareas/${testTareaId}/ejecutar`, { decision: 'aprobar' }),
    authenticatedApiClient.patch(`/api/tareas/${testTareaId}/ejecutar`, { decision: 'rechazar' }),
  ]);
  const exitosas = [r1, r2].filter((r) => !r.error);
  expect(exitosas.length).toBe(1); // solo una debe tener éxito
});
```

### 7.2 Agenda, Noticias y Comunicaciones (`12-agenda.spec.ts`, `13-noticias-comunicaciones.spec.ts` — nuevos)
- **CU-049** Crear evento de agenda
- **CU-050** Actualizar evento (incluye recurrencia: `reglaRecurrencia`)
- **CU-051** Consultar eventos (por rango de fechas)
- **CU-052** Eliminar evento (y sus excepciones de recurrencia)
- **CU-053** Crear noticia
- **CU-054** Actualizar noticia
- **CU-055** Consultar noticias
- **CU-056** Eliminar noticia (cascada elimina `comentarios` por FK)
- **CU-057** Comentar noticia
- **CU-058** Enviar notificación (`enviado: false → true`, `fechaEnvio` se establece)
- **CU-059** Consultar notificaciones (por destinatario / estado de envío)

### 7.3 Reportes y Auditoría (`14-reportes.spec.ts`, `09-auditoria-eventos.spec.ts`)
- **CU-060** Crear categoría de reporte
- **CU-061** Consultar categorías
- **CU-062** Ejecutar reporte (vía `url` de `reportes_ssrs` — probablemente proxy a un servidor SSRS externo; considerar mock)
- **CU-063** Consultar eventos de auditoría (filtrar por `tabla` + `id_registro` + `accion`)
- **CU-064** Consultar eventos de identidad (mismo patrón, pero `id_registro` es varchar — viene de `users.id`)

### 7.4 Listas / Catálogos (`05-formularios-campos.spec.ts` o nuevo `15-listas.spec.ts`)
- **CU-065** Crear lista y agregar ítems jerárquicos
- **CU-066** Consultar ítems de lista en cascada (relevante para `campos.cascada_de`, `campo1`, `campo2`)

---

## 8. Reglas de negocio — confirmadas vs. inferidas

| ID | Regla | Confirmada en DDL |
|---|---|---|
| RN-001 | Cada empresa tiene base de datos separada | ✅ Coherente con ausencia de `id_empresa` en tablas de negocio (formularios, workflow, etc.) |
| RN-002 | Usuarios pertenecen a una empresa | ⚠️ No vía columna; se infiere por aislamiento de BD (RN-001) |
| RN-003 | Sucursales pertenecen a una empresa | ✅ FK `sucursales.id_empresa` |
| RN-004 | Roles son por empresa | ✅ tabla `roles_empresa` |
| RN-005 | Código de formulario único | ⚠️ No hay `UNIQUE` en DDL — regla de aplicación |
| RN-006 | Campos tienen orden específico | ⚠️ `orden` es INTEGER libre, sin constraint de secuencia — la API debe garantizarlo |
| RN-009 | Directorios tienen padre (jerarquía) | ✅ `id_directorio_padre` |
| RN-010 | Código de directorio único por formulario | ⚠️ No hay `UNIQUE` compuesto en DDL |
| RN-011 | Control de versiones de documentos | ✅ `archivos.id_primera_version`, `es_version_actual` |
| RN-013/014 | TRD define retención y disposición | ✅ `trd.retencion_ag`, `retencion_ac`, `eliminacion`, etc. |
| RN-015 | Proceso tiene pasos secuenciales | ✅ FK `paso.id_proceso`; el orden lo da el campo `paso` (INTEGER) |
| RN-018 | Responsable único por paso | ⚠️ `tareas.responsable` es un solo varchar — un grupo (`grupos_wf`) podría implicar varios responsables posibles; confirmar con backend cómo se resuelve |
| RN-019 | Devolución de tareas permitida | ✅ tabla `posibles_pasos_devolucion` valida destinos permitidos |

---

## 9. Estructura de carpetas de pruebas (ampliada respecto a `playwrigth.md`)

```
tests/
├── e2e/
│   ├── 01-empresas.spec.ts
│   ├── 02-sucursales.spec.ts
│   ├── 03-identidad.spec.ts
│   ├── 04-multi-tenant-aislamiento.spec.ts   # usa TENANTS de §5, no filtro idEmpresa
│   ├── 05-formularios-campos.spec.ts
│   ├── 06-gestion-documental.spec.ts          # incluye TRD (CU-032)
│   ├── 07-workflow-tareas.spec.ts             # CU-033 a CU-046
│   ├── 08-concurrencia-workflow.spec.ts
│   ├── 09-auditoria-eventos.spec.ts           # CU-063, CU-064
│   ├── 10-importar-masivo.spec.ts
│   ├── 11-integracion-catalogos.spec.ts
│   ├── 12-agenda.spec.ts                      # NUEVO — CU-049 a CU-052
│   ├── 13-noticias-comunicaciones.spec.ts     # NUEVO — CU-053 a CU-059
│   ├── 14-reportes.spec.ts                    # NUEVO — CU-060 a CU-062
│   ├── 15-listas-catalogos.spec.ts            # NUEVO — CU-065, CU-066
│   └── 16-lotes-grupos-workflow.spec.ts       # NUEVO — lote_wf, grupos_wf, evidencias
├── fixtures/
│   ├── api.fixture.ts
│   ├── auth.fixture.ts
│   ├── catalogos.fixture.ts                   # ampliar con testProcesoId, testPasoId, testNoticiaId, testCategoriaReporteId, testListaId, testGrupoWfId, testLoteWfId
│   └── tenants.fixture.ts                     # NUEVO — TENANTS (EMP001/EMP002), ver §5
└── helpers/
    ├── api-client.ts
    ├── test-data.ts                           # fixtures de §6
    └── assertions.ts
```

---

## 10. Convenciones (heredadas de `playwrigth.md`)

- **Arrange / Act / Assert** en cada test.
- **Fixtures** para seed/cleanup automático (aprovechar `ON DELETE CASCADE` — §3 — para simplificar el cleanup).
- **Multi-tenant** validado por conexión/entorno separado, no por filtro `idEmpresa` (§1.3, §5).
- **Idioma**: descripciones en español (dominio), código en inglés (técnico).

---

## 11. Próximos pasos / validaciones pendientes con el equipo de backend

1. Confirmar el mecanismo real de "desactivar empresa" (no hay columna `activo`; ¿es soft-delete a otro nivel, o el CU-004 del análisis previo no aplica?).
2. Confirmar tabla de valores reales para los códigos catalogados: `formularios.estado`, `campos.tipo_dato`, `campos.control`, `directorios.id_estado`, `tareas.prioridad`/`tipo`, `workflow.estado` (posiblemente residen en `parametros`, agrupados por `grupo`).
3. Confirmar si la unicidad de `nit`/`codigo` se valida en la API (RN-005, RN-010) y qué mensaje/código de error devuelve.
4. Confirmar el contrato exacto de la API (nombres camelCase, rutas, paginación) — este documento asume convenciones .NET típicas, no las extrae literalmente de un Swagger.
5. Confirmar cómo se resuelve el aislamiento multi-tenant en el ambiente de pruebas (¿dos URLs/bases de datos físicas, o un solo endpoint con cabecera de tenant?). Esto determina si `tenants.fixture.ts` necesita una sola `baseURL` con header `X-Tenant` en vez de dos `baseURL` distintas.
