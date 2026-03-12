import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';

// Spec ref: Architecture Spec Section 1 — OIDC redirect_uri must match a registered route.
// /callback handles the Authorization Code exchange after Keycloak redirects back.
export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () =>
      import('./features/login/login.component').then((m) => m.LoginComponent),
  },
  {
    // OIDC redirect_uri target — must match authConfig.redirectUrl (/callback)
    path: 'callback',
    loadComponent: () =>
      import('./features/callback/callback.component').then(
        (m) => m.CallbackComponent
      ),
  },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./features/dashboard/dashboard.component').then(
        (m) => m.DashboardComponent
      ),
    canActivate: [authGuard],
  },
  { path: '**', redirectTo: 'dashboard' },
];
