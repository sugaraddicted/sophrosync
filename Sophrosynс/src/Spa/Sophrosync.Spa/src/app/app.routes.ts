import { Routes } from '@angular/router';
import { ShellComponent } from './shell/shell.component';

export const routes: Routes = [
  {
    path: '',
    component: ShellComponent,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
      },
      {
        path: 'clients',
        loadComponent: () =>
          import('./features/clients/clients.component').then(m => m.ClientsComponent),
      },
      {
        path: 'calendar',
        loadComponent: () =>
          import('./features/calendar/calendar.component').then(m => m.CalendarComponent),
      },
      {
        path: 'notes',
        children: [
          {
            path: '',
            loadComponent: () =>
              import('./features/notes/notes.component').then(m => m.NotesComponent),
          },
          {
            path: 'new',
            loadComponent: () =>
              import('./features/notes/notes-editor/notes-editor.component').then(m => m.NotesEditorComponent),
          },
          {
            path: ':id',
            loadComponent: () =>
              import('./features/notes/notes-editor/notes-editor.component').then(m => m.NotesEditorComponent),
          },
        ],
      },
      {
        path: 'settings',
        loadComponent: () =>
          import('./features/settings/settings.component').then(m => m.SettingsComponent),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
