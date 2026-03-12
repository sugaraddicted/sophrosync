import { Injectable, inject, signal, effect } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';

// Spec ref: Architecture Spec Section 1 (OIDC Authorization Code + PKCE)
// Section 2.2 (JWT payload: sub, tenant_id, roles, email, preferred_username, given_name, family_name)
// Section 2.3 (.NET Integration: Keycloak RS256 JWT, realm sophrosync)
//
// Replaced: ROPC grant_type=password flow (insecure, deprecated, violates spec Section 1).
// Now delegates all auth to OidcSecurityService (angular-auth-oidc-client) which handles:
//   - Authorization Code + PKCE code_verifier/code_challenge generation
//   - Token storage (library-managed, not manual sessionStorage)
//   - Silent renew via refresh token
export interface UserProfile {
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  tenantId: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly oidc = inject(OidcSecurityService);

  readonly isAuthenticated = signal(false);
  readonly userProfile = signal<UserProfile | null>(null);
  readonly userRoles = signal<string[]>([]);

  constructor() {
    // Subscribe to OIDC state changes and propagate to Angular signals
    this.oidc.isAuthenticated$.subscribe(({ isAuthenticated }) => {
      this.isAuthenticated.set(isAuthenticated);
    });

    this.oidc.userData$.subscribe(({ userData }) => {
      if (!userData) {
        this.userProfile.set(null);
        this.userRoles.set([]);
        return;
      }

      this.userProfile.set({
        username: userData['preferred_username'] ?? '',
        email: userData['email'] ?? '',
        firstName: userData['given_name'] ?? '',
        lastName: userData['family_name'] ?? '',
        // Spec Section 2.2: tenant_id is a custom claim injected by Keycloak Protocol Mapper
        tenantId: userData['tenant_id'] ?? '',
      });

      // Spec Section 2.2: roles array in JWT payload (also available via realm_access.roles)
      const roles: string[] =
        userData['roles'] ?? userData['realm_access']?.roles ?? [];
      this.userRoles.set(roles);
    });
  }

  /** Initiates Authorization Code + PKCE flow — redirects to Keycloak login page. */
  login(): void {
    this.oidc.authorize();
  }

  /** Logs out and redirects to Keycloak logout endpoint, then to /login. */
  logout(): void {
    this.oidc.logoff().subscribe();
  }

  /** Returns the current access token for use in HTTP interceptor. */
  getToken(): string {
    return this.oidc.getAccessToken();
  }

  /** Called on app init to check and restore any existing OIDC session. */
  restoreSession(): Promise<void> {
    return new Promise((resolve) => {
      this.oidc.checkAuth().subscribe(() => resolve());
    });
  }
}
