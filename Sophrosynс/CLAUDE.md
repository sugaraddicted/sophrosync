# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## Commands

### Backend (.NET 8)

```bash
# Build the entire solution
dotnet build Sophrosynс.sln

# Run a single service (from its API project directory)
dotnet run --project src/Services/ClientService/Sophrosync.Clients.API

# Add an EF Core migration (from the service's Infrastructure project directory)
dotnet ef migrations add <MigrationName> --startup-project ../Sophrosync.Clients.API

# Apply migrations
dotnet ef database update --startup-project ../Sophrosync.Clients.API
```

Expected build result: **0 errors, 4 warnings** (NU1603 — QuestPDF version resolution, harmless).

### Frontend (Angular 21 + Vitest)

All commands run from `src/Spa/Sophrosync.Spa/`:

```bash
npm install
ng serve          # Dev server at http://localhost:4200, proxies /api/* to gateway
ng build          # Production build to dist/
ng test           # Unit tests with Vitest
```

### Infrastructure (Docker)

```bash
# Start required infrastructure (postgres + keycloak) — always needed
docker compose up postgres keycloak -d

# Optionally containerise individual services (vs running them from VS/CLI)
docker compose --profile clients up -d
docker compose --profile notes up -d
docker compose --profile gateway up -d
docker compose --profile services up -d   # all services
```

Keycloak admin: `http://localhost:8080` — credentials in `.env` (`KC_ADMIN_PASSWORD`).
Postgres: `localhost:5432`, user `sophrosync`.

---

## Architecture

### Backend — Clean Architecture per Service

Each microservice follows a strict 4-layer structure:

```
Sophrosync.<Name>.Domain          # Aggregates, entities, domain interfaces, value objects
Sophrosync.<Name>.Application     # CQRS commands/queries, DTOs, validators, handlers
Sophrosync.<Name>.Infrastructure  # EF Core DbContext, repositories, migrations
Sophrosync.<Name>.API             # ASP.NET Core controllers, Program.cs
```

**Service ports (development):** ClientService → 5001, NotesService → 5003, Gateway → 5000, Keycloak → 8080.

### CQRS Pattern

All business logic flows through MediatR. Commands and queries live in `Application/Commands/<Name>/` and `Application/Queries/<Name>/`. Each folder contains the command/query record, its handler, and optionally a FluentValidation validator.

`ValidationBehavior` and `LoggingBehavior` are registered globally via `cfg.AddBehavior(...)` in `Program.cs` — don't add inline validation in handlers.

Validation errors surface as HTTP 400 via the exception handler in `Program.cs` that catches `FluentValidation.ValidationException`.

### SharedKernel

`src/Shared/Sophrosync.SharedKernel/` provides cross-cutting concerns consumed by all services:

| Namespace | Contents |
|---|---|
| `Domain` | `Entity` (Id, CreatedAt, UpdatedAt, TouchUpdatedAt), `AggregateRoot`, `ValueObject` |
| `Abstractions` | `ICurrentUser`, `ICurrentTenant`, `IRepository`, `IDomainEvent` |
| `Services` | `CurrentUserService`, `CurrentTenantService` — reads JWT claims from `IHttpContextAccessor` |
| `Behaviors` | `ValidationBehavior<,>`, `LoggingBehavior<,>` |
| `Security` | `EncryptedStringConverter` (AES-256-GCM), `KeycloakClaimsExtensions`, `KeycloakRolesTransformation` |
| `Http` | `ResiliencePolicy` (Polly retry + circuit breaker), `ITypedServiceClient` |

**SharedKernel ProjectReference path** from any service layer: `..\..\..\..\Shared\Sophrosync.SharedKernel\Sophrosync.SharedKernel.csproj` — note: service layers are nested 3 directories inside `src/Services/`, so it's 4 `..` segments up to reach `src/`.
Correct path template: `..\..\..\Shared\Sophrosync.SharedKernel\Sophrosync.SharedKernel.csproj` (3 levels up from the _service layer project_, landing at the `Services/` directory, then one more into Shared — verify per-service).

### PHI Encryption (Critical)

All PHI columns (client name, email, phone; note title, content; notification body; etc.) use `EncryptedStringConverter` — an EF Core `ValueConverter` wrapping AES-256-GCM. The key is a 32-byte base64 string from configuration (`Encryption:<ServiceName>Key`).

**Important:** Never call `modelBuilder.ApplyConfigurationsFromAssembly()` in a DbContext that has encrypted columns — it invokes the parameterless constructor with a placeholder key. Always register configurations explicitly:

```csharp
modelBuilder.ApplyConfiguration(new ClientConfiguration(encryptionOptions.Key));
```

Pass the key via a singleton options class (`ClientsEncryptionOptions`, etc.) registered in `ServiceCollectionExtensions`.

### Multi-Tenancy & Soft Deletes

Every DbContext has a combined EF Core global query filter on every aggregate root:

```csharp
modelBuilder.Entity<Client>()
    .HasQueryFilter(e => !e.IsDeleted && e.TenantId == currentTenant.Id);
```

`ICurrentTenant` resolves the tenant from the JWT `tenant_id` claim. Never filter by tenant manually in queries — the global filter handles it automatically.

`SoftDelete()` sets `IsDeleted = true` and `DeletedAt = DateTime.UtcNow` on the entity. Physically deleting rows is not permitted for PHI data.

### Keycloak & RBAC

Realm: `sophrosync`. Roles: `admin`, `supervisor`, `therapist`, `client`.

Keycloak emits roles in a `roles` claim (not `ClaimTypes.Role`). Authorization policies must target this claim:

```csharp
options.AddPolicy("CanManageClients", p =>
    p.RequireClaim("roles", "admin", "therapist", "supervisor"));
```

`KeycloakRolesTransformation` transforms the `roles` claim into `ClaimTypes.Role` — register it via `services.AddScoped<IClaimsTransformation, KeycloakRolesTransformation>()` (see NotesService `Program.cs` for the pattern).

TenantId comes from the `tenant_id` JWT claim — read by `CurrentTenantService`.

### YARP Gateway

`src/Gateway/Sophrosync.Gateway/` proxies all traffic. Public routes match `/api/<service>/**`; inter-service routes match `/internal/<service>/**` and carry `"Metadata": { "internal": "true" }`. Adding a new service requires a new route + cluster entry in `appsettings.json`.

### Angular SPA

**Location:** `src/Spa/Sophrosync.Spa/`

The shell layout (`app/shell/shell.component`) renders the side nav and `<router-outlet>`. All feature pages are lazy-loaded via `loadComponent` in `app.routes.ts` and nested under the `ShellComponent` route.

**State management:** Angular Signals (`signal`, `computed`, `effect`). No NgRx. All HTTP calls use `HttpClient` injected via `inject()` in service classes decorated `@Injectable({ providedIn: 'root' })`.

**API calls:** All services call `/api/<resource>` — proxied to the gateway during dev (no proxy config file found; relies on the gateway running on port 5000 or Angular dev server proxy configuration).

**Standalone components:** All components use `standalone: true` and declare their own imports. No feature modules.

**Design system — "Modern Archivist":**
- Tokens defined in `src/styles.scss` as CSS custom properties
- Fonts: `Newsreader` (serif, headlines, italic style) / `Work Sans` (sans-serif, body)
- Palette: Sage Green primary (`--color-primary: #546253`), warm grey secondary, dusty rose tertiary
- Radius: max `0.5rem` — no pill shapes
- Icons: Google Material Symbols Outlined (`font-variation-settings: 'FILL' 0, 'wght' 300`)
- Shared feature page layout: `@use '_feature-page'` — provides `.feature-page`, `.feature-hero`, `.feature-hero__headline`, `.placeholder-card`

### Notes Feature — Lifecycle State Machine

Notes (`NoteStatus`): `Draft → PendingCoSign → Signed → Locked → Amended`

Key rules enforced in the domain:
- Only `Draft` notes can be updated; `Locked` and `Amended` are read-only
- Only `Signed` notes can be locked
- `Amend()` marks the original `Locked` note as `Amended` and creates a new `Draft` with `AmendedFromId`
- Only `Draft` notes can be soft-deleted

The SPA editor (`notes-editor.component.ts`) has autosave (3s debounce) for existing notes and a two-step confirmation flow for locking.

---

## Development Workflow

For local dev, start infrastructure first:

```bash
docker compose up postgres keycloak -d
```

Then run backend services from Visual Studio (launch profile) or `dotnet run`. The SPA dev server proxies API calls to the gateway. Scalar API reference is available at `/scalar/v1` in Development mode for each service.
