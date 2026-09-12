import { HttpInterceptorFn } from '@angular/common/http';

const TOKEN_KEY = 'energysuite_token';
const TENANT_KEY = 'energysuite_tenant_id';
const DEFAULT_TENANT_ID = '00000000-0000-0000-0000-000000000001';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = sessionStorage.getItem(TOKEN_KEY) || localStorage.getItem(TOKEN_KEY);
  const tenantId = localStorage.getItem(TENANT_KEY) || DEFAULT_TENANT_ID;

  let headers = req.headers;

  if (token && !headers.has('Authorization')) {
    headers = headers.set('Authorization', `Bearer ${token}`);
  }

  if (!headers.has('X-Tenant-ID')) {
    headers = headers.set('X-Tenant-ID', tenantId);
  }

  const authReq = req.clone({ headers });
  return next(authReq);
};
