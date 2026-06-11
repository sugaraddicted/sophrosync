# Sophrosync

Practice management SaaS for private therapists — built as a Bachelor of Computer Engineering diploma project.

Therapists manage clients, session notes, appointments, consent records, and notifications through a secure multi-tenant web application.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | .NET 8 microservices, Clean Architecture, CQRS + MediatR |
| Database | PostgreSQL 16 (one DB per service), EF Core + Npgsql |
| Auth | Keycloak 26 (RS256 JWT, OIDC, RBAC) |
| Security | AES-256-GCM PHI column encryption, EF Core global tenant filters |
| Gateway | YARP reverse proxy, rate limiting, correlation ID |
| Frontend | Angular 21, Signals, Tailwind CSS v4 |
| Infrastructure | Docker Compose, MailHog (dev SMTP) |

---

## Architecture

```
Browser → [Angular SPA :4200]
              ↓  HTTPS / OIDC
         [Keycloak :8080]  ──────────────────────────────┐
              ↓  JWT (RS256)                              │
         [YARP Gateway :5000]                             │
              ↓  JWT validation, rate limiting, routing   │
    ┌─────────┬──────────┬──────────┬──────────┬──────────┴──────────┐
    ▼         ▼          ▼          ▼          ▼                     ▼
Client     Schedule    Notes     Consent  Notification           Reporting
(:5001)    (:5002)    (:5003)   (:5006)    (:5007)               (:5008)
    └───────────────────── PostgreSQL :5432 ─────────────────────────┘
```

Each service uses a private database — cross-service reads go through HTTP, never direct joins.

---

## Services

| Service | Port | Bounded Context |
|---|---|---|
| YARP Gateway | 5000 | JWT validation, routing, rate limiting, tenant provisioning |
| ClientService | 5001 | Patient records (PHI), diagnoses, risk levels |
| ScheduleService | 5002 | Appointments, therapist availability templates |
| NotesService | 5003 | Session notes (Draft → Signed → Locked), treatment plans |
| ConsentService | 5006 | GDPR Art. 7 consent lifecycle — append-only records |
| NotificationService | 5007 | Email + in-app notifications, user preferences |
| ReportingService | 5008 | Clinical outcomes, practice analytics, GDPR RoPA |
| SharedKernel | — | Shared base entities, behaviors, encryption, HTTP resilience |

---

## Security

- **Multi-tenancy** — `tenant_id` JWT claim + EF Core global query filter on every aggregate
- **PHI encryption** — AES-256-GCM `ValueConverter` on sensitive columns (names, notes, emails)
- **RBAC** — Keycloak roles (`admin`, `supervisor`, `therapist`, `client`) mapped to authorization policies
- **Consent records** — INSERT-only at the DB level; application-level writes enforce append-only
- **Soft delete** — all client data uses `IsDeleted` / `DeletedAt` (GDPR Art. 17 right to erasure)
- **Audit trail** — AuditService with INSERT-only DB role, records every PHI mutation

---

## Repository Layout

```
Sophrosynс\          ← backend .NET solution (Sophrosynс.sln)
  src/
    Shared/
      Sophrosync.SharedKernel/
    Gateway/
      Sophrosync.Gateway/
    Services/
      ClientService/        ← Domain / Application / Infrastructure / API
      ScheduleService/
      NotesService/
      ConsentService/
      NotificationService/
      ReportingService/
      IdentityService/

Sophrosync.Spa\      ← Angular 21 SPA
  src/app/
    core/auth/       ← Keycloak ROPC flow, interceptors, guards
    layout/          ← ShellLayoutComponent (nav + router-outlet)
    features/        ← clients, notes, calendar, dashboard, settings

Sophrosync.Obs\      ← Obsidian wiki (architecture docs, specs, learnings)

Sophrosync Backend Architecture Spec.md   ← full architecture reference
```

---

## Getting Started

### Prerequisites

- Docker Desktop
- .NET 8 SDK
- Node.js 20+ / npm

### 1. Start infrastructure

```bash
cd Sophrosynс
docker compose up postgres keycloak mailhog -d
```

Keycloak imports the `sophrosync` realm automatically on first start.

| Service | URL |
|---|---|
| Keycloak admin | `http://localhost:8080` |
| MailHog (dev email) | `http://localhost:8025` |

### 2. Run backend services

From Visual Studio, use the launch profiles. Or from the CLI:

```bash
# Example: ClientService
dotnet run --project src/Services/ClientService/Sophrosync.Clients.API

# Example: Gateway
dotnet run --project src/Gateway/Sophrosync.Gateway
```

Or run all services as containers:

```bash
docker compose --profile services up -d
```

### 3. Run the Angular SPA

```bash
cd Sophrosync.Spa
npm install
npm start          # dev server at http://localhost:4200
```

The dev server proxies `/api/*` to the YARP Gateway at `:5000`.

### 4. Run EF Core migrations

```bash
# From a service's Infrastructure project directory, e.g.:
cd src/Services/ClientService/Sophrosync.Clients.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../Sophrosync.Clients.API
dotnet ef database update --startup-project ../Sophrosync.Clients.API
```

---

## Development

### Backend build

```bash
dotnet build Sophrosynс.sln
# Expected: 0 errors, 4 warnings (NU1603 — harmless QuestPDF version resolution)
```

### Tests

```bash
dotnet test Sophrosynс.sln
```

Integration tests use Testcontainers (real PostgreSQL + Keycloak spun up per test run).

### Frontend tests

```bash
cd Sophrosync.Spa
ng test       # Vitest
ng e2e        # e2e (configure your runner)
```

### Scalar API reference

Each service exposes Scalar at `/scalar/v1` in Development mode.

---

## Clean Architecture Pattern

Every service follows the same four-layer structure:

```
Sophrosync.<Name>.Domain          # Aggregates, value objects, domain events, interfaces
Sophrosync.<Name>.Application     # CQRS commands/queries, handlers, validators, DTOs
Sophrosync.<Name>.Infrastructure  # EF Core DbContext, repositories, migrations
Sophrosync.<Name>.API             # Controllers, Program.cs, DI wiring
```

Cross-cutting concerns (validation, logging, exception mapping) are handled by three MediatR pipeline behaviors in SharedKernel — `ValidationBehavior`, `LoggingBehavior`, `ExceptionBehavior`.

---

## Environment Variables

Key environment variables for each service (see `docker-compose.yml` for the full set):

| Variable | Purpose |
|---|---|
| `ConnectionStrings__<Service>Db` | PostgreSQL connection string |
| `Keycloak__Authority` | Keycloak realm URL for JWT validation |
| `Keycloak__Audience` | Expected JWT audience (`sophrosync-gateway`) |
| `Encryption__<Service>Key` | 32-byte base64 AES-256-GCM key for PHI columns |
| `Smtp__Host` / `Smtp__Port` | SMTP config for NotificationService |

Copy `.env.example` to `.env` and fill in passwords before running Docker Compose.
