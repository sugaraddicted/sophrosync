import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () =>
      import('./features/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: '',
    loadComponent: () =>
      import('./layout/shell/shell-layout.component').then(
        (m) => m.ShellLayoutComponent
      ),
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then(
            (m) => m.DashboardComponent
          ),
      },
      {
        path: 'clients',
        redirectTo: 'dashboard',
      },
      {
        path: 'schedule',
        redirectTo: 'dashboard',
      },
      {
        path: 'notes',
        redirectTo: 'dashboard',
      },
    ],
  },
  { path: '**', redirectTo: 'dashboard' },
];
