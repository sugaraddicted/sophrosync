import { APP_INITIALIZER, ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAuth } from 'angular-auth-oidc-client';

import { routes } from './app.routes';
import { AuthService } from './core/auth/auth.service';
import { authInterceptor } from './core/auth/auth.interceptor';
import { authConfig } from './core/auth/auth.config';

// Spec ref: Architecture Spec Section 1 (OIDC Authorization Code + PKCE)
// provideAuth wires OidcSecurityService with Keycloak PKCE config.
// APP_INITIALIZER restores any existing OIDC session on startup.
export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideAuth(authConfig),
    {
      provide: APP_INITIALIZER,
      useFactory: (auth: AuthService) => () => auth.restoreSession(),
      deps: [AuthService],
      multi: true,
    },
  ],
};
