import { PassedInitialConfig } from 'angular-auth-oidc-client';
import { environment } from '../../../environments/environment';

// Spec ref: Architecture Spec Section 1 (OIDC Authorization Code + PKCE flow diagram)
// Section 2.2 (JWT payload: sub, tenant_id, roles, email, iss)
// Keycloak realm: sophrosync — single realm, tenant_id as custom claim via Protocol Mapper
export const authConfig: PassedInitialConfig = {
  config: {
    authority: `${environment.keycloak.url}/realms/${environment.keycloak.realm}`,
    redirectUrl: `${window.location.origin}/callback`,
    postLogoutRedirectUri: `${window.location.origin}/login`,
    clientId: environment.keycloak.clientId,
    scope: 'openid profile email',
    responseType: 'code',
    silentRenew: true,
    useRefreshToken: true,
    renewTimeBeforeTokenExpiresInSeconds: 30,
    ignoreNonceAfterRefresh: true,
    // Do not store tokens in sessionStorage manually — library handles storage
    secureRoutes: [environment.apiUrl],
  },
};
