import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';

// Spec ref: Architecture Spec Section 2.3 — all API requests must carry the Keycloak JWT
// as a Bearer token so YARP Gateway can validate it (RS256 via JWKS auto-discovery).
// OWASP A07: token provided by OidcSecurityService — no manual sessionStorage access.
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  if (!auth.isAuthenticated()) {
    return next(req);
  }

  const token = auth.getToken();
  if (!token) {
    return next(req);
  }

  return next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
};
