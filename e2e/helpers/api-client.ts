import type { APIRequestContext, APIResponse } from '@playwright/test';

export interface OpcionesCliente {
  /** Token JWT a enviar como `Authorization: Bearer`. */
  token?: string;
  /** Empresa activa (cabecera `X-Id-Empresa`) para resolver el tenant. */
  idEmpresa?: number;
}

/**
 * Envoltura ligera sobre el `request` de Playwright que adjunta el header de
 * autenticación y la cabecera multi-tenant `X-Id-Empresa` en cada llamada.
 *
 * El aislamiento multi-tenant se prueba alternando `idEmpresa`, no usando dos
 * baseURL distintas (ver MiddlewareDeTenant del backend).
 */
export class ApiClient {
  constructor(
    private readonly request: APIRequestContext,
    private readonly opciones: OpcionesCliente = {},
  ) {}

  /** Devuelve un cliente nuevo con las opciones combinadas (token/empresa). */
  con(opciones: OpcionesCliente): ApiClient {
    return new ApiClient(this.request, { ...this.opciones, ...opciones });
  }

  private headers(): Record<string, string> {
    const h: Record<string, string> = {};
    if (this.opciones.token) h['Authorization'] = `Bearer ${this.opciones.token}`;
    if (this.opciones.idEmpresa != null) h['X-Id-Empresa'] = String(this.opciones.idEmpresa);
    return h;
  }

  get(url: string): Promise<APIResponse> {
    return this.request.get(url, { headers: this.headers() });
  }

  post(url: string, data?: unknown): Promise<APIResponse> {
    return this.request.post(url, { headers: this.headers(), data: data as object });
  }

  put(url: string, data?: unknown): Promise<APIResponse> {
    return this.request.put(url, { headers: this.headers(), data: data as object });
  }

  del(url: string): Promise<APIResponse> {
    return this.request.delete(url, { headers: this.headers() });
  }
}
