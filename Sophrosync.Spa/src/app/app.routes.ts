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
    path: 'register',
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/registration/register/register.component').then(
            (m) => m.RegisterComponent
          ),
      },
      {
        path: 'confirmation',
        loadComponent: () =>
          import('./features/registration/email-confirmation/email-confirmation.component').then(
            (m) => m.EmailConfirmationComponent
          ),
      },
    ],
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
        loadComponent: () =>
          import('./features/clients/clients-page/clients-page.component').then(
            (m) => m.ClientsPageComponent
          ),
      },
      {
        path: 'calendar',
        loadComponent: () =>
          import('./features/calendar/calendar.component').then(
            (m) => m.CalendarComponent
          ),
      },
      {
        path: 'notes',
        loadComponent: () =>
          import('./features/notes/notes.component').then(
            (m) => m.NotesComponent
          ),
      },
      {
        path: 'settings',
        loadComponent: () =>
          import('./features/settings/settings.component').then(
            (m) => m.SettingsComponent
          ),
      },
    ],
  },
  { path: '**', redirectTo: 'dashboard' },
];
