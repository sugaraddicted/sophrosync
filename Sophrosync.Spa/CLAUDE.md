# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Development server (with backend proxy)
npm start               # ng serve — http://localhost:4200

# Build
npm run build           # production build → dist/
npm run watch           # development build with watch

# Tests
npm test                # vitest via ng test

# Angular code generation
npx ng generate component features/<name>/<name>.component --standalone
npx ng generate service features/<name>/<name>.service
```

## Architecture

### App structure

```
src/app/
  app.config.ts          # App-level providers: router, HttpClient, authInterceptor, APP_INITIALIZER
  app.routes.ts          # Top-level lazy routes
  core/auth/             # AuthService, authGuard, roleGuard, authInterceptor
  layout/                # ShellLayoutComponent (header + side-menu + router-outlet)
  features/
    dashboard/           # Appointments calendar + next-session card
    clients/             # CRUD: ClientsService, ClientsPageComponent, modals
    notes/               # NotesService, NotesComponent, note-form/detail modals
    calendar/            # Stub CalendarComponent
    settings/            # Stub SettingsComponent
    login/               # LoginComponent
```

### Routing pattern

All protected routes are children of the lazy-loaded `ShellLayoutComponent`, guarded by `authGuard`. The shell holds the header, collapsible side-menu, and `<router-outlet>`. The `login` route sits outside the shell.

### Auth

Custom Keycloak ROPC flow (no PKCE library). `AuthService` exchanges username/password for tokens directly against the Keycloak token endpoint. The `refresh_token` is stored in `sessionStorage` under key `sophrosync_rt`. On app init, `APP_INITIALIZER` calls `auth.restoreSession()` to silently refresh. The `authInterceptor` attaches `Authorization: Bearer <token>` to all requests when authenticated.

Environment config lives in `src/environments/environment.development.ts`:
- Keycloak: `http://localhost:8080`, realm `sophrosync`, client `sophrosync-spa`
- `apiUrl: '/api'` (proxied to the backend)

### Backend proxy

`proxy.conf.json` routes during `ng serve`:
- `/api/clients` → `http://localhost:5001` (ClientService)
- `/api/notes` → `http://localhost:5003` (NotesService)
- `/api` (catch-all) → `http://localhost:60228` (YARP Gateway)

### Component conventions

- All components are **standalone** (no NgModules).
- Use `ChangeDetectionStrategy.OnPush` on all components.
- State is managed with Angular **signals** (`signal()`, `computed()`).
- Services use `inject()` for dependency injection, not constructor injection.
- Services are `providedIn: 'root'`.

### Styling system

**Tailwind CSS v4** with `@theme {}` in `src/styles.scss` defining the full "Modern Archivist" design token set (colors, typography, radius, shadows). Key tokens:

- `--color-primary` (#546253 sage green), `--color-background` (#fafaf5), `--color-surface-container-lowest` (white)
- `--font-headline` (Newsreader serif), `--font-sans` (Work Sans)
- `--radius-full` is intentionally capped at `0.25rem` — no pill/fully-rounded shapes

SCSS mixins are in `src/styles/_mixins.scss`. Because `angular.json` sets `stylePreprocessorOptions.includePaths: ["src"]`, import them as:
```scss
@use 'styles/mixins' as m;
```

Key mixins: `card-base`, `card-heading`, `ghost-border`, `section-surface`.

Icons: **Material Symbols Outlined** loaded via Google Fonts. Use `<span class="material-symbols-outlined">icon_name</span>`. Default weight is 300 / 24px per global styles.

### Generating new feature pages

1. Create `src/app/features/<name>/<name>.component.ts` (standalone, OnPush).
2. Add a lazy route in `app.routes.ts` as a child of the shell route.
3. Add a nav entry in `app-side-menu.component`.
4. Create a `<name>.service.ts` if the feature calls the backend; base URL from `environment.apiUrl`.
