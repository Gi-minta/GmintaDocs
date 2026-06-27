import { Client } from 'pg';

/**
 * Limpieza global tras la suite E2E.
 *
 * La suite siembra empresas de prueba en la BD maestra y aprovisiona una BD
 * por-tenant (`gmintadocs_empresa_{id}`) para varias de ellas. Nada de eso se
 * borra solo: el `DELETE`/`aprovisionar` de la API no dropea la BD por-tenant,
 * y no hay endpoint de borrado masivo. Este teardown restablece el entorno a
 * su estado sembrado: deja únicamente la empresa demo (id 1) y su BD.
 *
 * Conexión: usa las variables PG estándar; los valores por defecto coinciden
 * con el docker-compose del proyecto y con el service de Postgres del CI
 * (localhost:5433, postgres/postgres, BD maestra `gmintadocs_maestro`).
 *
 * Se puede saltar con `E2E_SKIP_TEARDOWN=1` (p. ej. para inspeccionar datos
 * tras una corrida fallida).
 */

/** Empresa que se conserva (la sembrada al arrancar). Resto se elimina. */
const KEEP_EMPRESA_ID = Number(process.env.KEEP_EMPRESA_ID ?? 1);

/** Patrón de las BD por-tenant; se valida antes de interpolar en el DROP. */
const PATRON_BD_TENANT = /^gmintadocs_empresa_(\d+)$/;

function configBase() {
  return {
    host: process.env.PGHOST ?? 'localhost',
    port: Number(process.env.PGPORT ?? 5433),
    user: process.env.PGUSER ?? 'postgres',
    password: process.env.PGPASSWORD ?? 'postgres',
  };
}

const DB_MAESTRA = process.env.PGDATABASE_MAESTRA ?? 'gmintadocs_maestro';
/** BD de mantenimiento (siempre existe) para poder dropear las demás. */
const DB_MANTENIMIENTO = 'postgres';

async function limpiarMaestra(): Promise<number> {
  const cliente = new Client({ ...configBase(), database: DB_MAESTRA });
  await cliente.connect();
  try {
    // Hijas con id_empresa (sin FK declarada) primero, luego las empresas.
    await cliente.query('BEGIN');
    await cliente.query('DELETE FROM sucursales WHERE id_empresa > $1', [KEEP_EMPRESA_ID]);
    await cliente.query('DELETE FROM roles_empresa WHERE id_empresa > $1', [KEEP_EMPRESA_ID]);
    await cliente.query('DELETE FROM radicados_empresa WHERE id_empresa > $1', [KEEP_EMPRESA_ID]);
    const res = await cliente.query('DELETE FROM empresas WHERE id_empresa > $1', [KEEP_EMPRESA_ID]);
    await cliente.query('COMMIT');
    return res.rowCount ?? 0;
  } catch (e) {
    await cliente.query('ROLLBACK').catch(() => {});
    throw e;
  } finally {
    await cliente.end();
  }
}

async function dropearBdTenant(): Promise<number> {
  const cliente = new Client({ ...configBase(), database: DB_MANTENIMIENTO });
  await cliente.connect();
  try {
    const { rows } = await cliente.query<{ datname: string }>(
      "SELECT datname FROM pg_database WHERE datname LIKE 'gmintadocs_empresa_%'",
    );
    let dropeadas = 0;
    for (const { datname } of rows) {
      const m = PATRON_BD_TENANT.exec(datname);
      if (!m || Number(m[1]) === KEEP_EMPRESA_ID) continue; // valida nombre y preserva la demo
      // Cerrar conexiones vivas (la API pudo dejar el pool abierto) antes del DROP.
      await cliente.query('SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = $1', [datname]);
      // datname ya validado contra PATRON_BD_TENANT → seguro interpolar (DDL no admite parámetros).
      await cliente.query(`DROP DATABASE IF EXISTS "${datname}"`);
      dropeadas++;
    }
    return dropeadas;
  } finally {
    await cliente.end();
  }
}

export default async function globalTeardown(): Promise<void> {
  if (process.env.E2E_SKIP_TEARDOWN === '1') {
    console.log('[teardown] omitido (E2E_SKIP_TEARDOWN=1).');
    return;
  }
  try {
    const empresas = await limpiarMaestra();
    const bds = await dropearBdTenant();
    console.log(`[teardown] limpieza OK: ${empresas} empresa(s) y ${bds} BD por-tenant eliminadas (se conserva id ${KEEP_EMPRESA_ID}).`);
  } catch (e) {
    // No reventar el resultado de la suite por un fallo de limpieza; solo avisar.
    console.warn(`[teardown] no se pudo limpiar: ${(e as Error).message}`);
  }
}
