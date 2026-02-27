import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getAccessToken();
  const tenantId = authService.tenantId();

  if (token) {
    let headers = req.headers.set('Authorization', `Bearer ${token}`);
    if (tenantId) {
      headers = headers.set('X-Tenant-Id', tenantId);
    }
    return next(req.clone({ headers }));
  }

  return next(req);
};
