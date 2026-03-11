# Sophrosync — Backend Architecture Specification

**Version:** 1.2  
**Stack:** .NET 8, PostgreSQL, Keycloak, Docker, Angular SPA (client)  
**Scope:** Backend microservices, patterns, entities, inter-service contracts

***

## 1. Architecture Overview

Sophrosync is a **security-focused SaaS platform** for psychotherapists, built as a set of .NET 8 microservices behind a YARP API Gateway. Identity is fully delegated to Keycloak. Services are containerized with Docker and share a PostgreSQL instance with one database per service.[^1][^2]

```
[ Angular SPA ]
      │  HTTPS / Authorization Code + PKCE (OIDC)
      ▼
[ Keycloak ]  ←── login, JWT issuance, MFA, user management
      │  JWT (RS256)
      ▼
[ API Gateway (YARP) ]  ←── JWT validation, rate limiting, routing
      │
  ┌───┴──────────────────────────────────────────────┐
  │              Internal HTTP (REST)                 │
  ▼         ▼          ▼          ▼         ▼
[Client] [Schedule]  [Notes] [Document]  [Audit]
  │          │          │          │         │
  └──────────┴──────────┴──────────┴─────────┘
                    PostgreSQL (per-service DB)
```

**Key decisions:**

- **Keycloak** owns all auth — .NET services are pure JWT resource servers, no auth code in services[^3][^4][^5]
- **Multi-tenancy:** Single Keycloak realm, `tenant_id` as a custom JWT claim injected via Protocol Mapper[^6][^7]
- **Inter-service communication:** Direct HTTP calls only — no message broker, no outbox pattern; complexity is not justified at this scope
- **Tenant provisioning:** Handled in Gateway middleware on first request — not a separate service
- **BillingService:** Deferred — out of MVP scope

***

## 2. Keycloak Setup

### 2.1 Realm & Multi-tenancy

**Single realm `sophrosync`, `tenant_id` as custom claim.**[^8][^7][^6]

Each practice = one Keycloak **Group**. `tenant_id` is injected into the JWT via a Protocol Mapper (User Attribute → claim). Roles defined at realm level: `admin`, `therapist`, `supervisor`, `assistant`.

> **Why not realm-per-tenant?** Keycloak degrades past ~100 realms and management overhead grows linearly. Single realm with tenant claim is the recommended SaaS pattern.[^7][^6]

### 2.2 JWT Payload (custom claims)

```json
{
  "sub": "keycloak-user-uuid",
  "tenant_id": "practice-uuid",
  "roles": ["therapist"],
  "email": "therapist@clinic.com",
  "iss": "https://keycloak:8080/realms/sophrosync"
}
```

### 2.3 .NET Integration

Same pattern in every service — resource server only, no login logic:[^4][^5][^3]

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://keycloak:8080/realms/sophrosync";
        options.Audience  = "sophrosync-gateway";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageClients",  p => p.RequireRole("therapist", "admin"));
    options.AddPolicy("CanSignNotes",      p => p.RequireRole("therapist", "supervisor"));
    options.AddPolicy("AdminOnly",         p => p.RequireRole("admin"));
});
```

### 2.4 Docker Compose Entry

```yaml
keycloak:
  image: quay.io/keycloak/keycloak:24.0
  command: start-dev
  environment:
    KEYCLOAK_ADMIN: admin
    KEYCLOAK_ADMIN_PASSWORD: admin
    KC_DB: postgres
    KC_DB_URL: jdbc:postgresql://postgres:5432/keycloak
    KC_DB_USERNAME: keycloak
    KC_DB_PASSWORD: keycloak
  ports:
    - "8080:8080"
```

***

## 3. Patterns Applied (per service)

### 3.1 Clean Architecture Layers

Every service follows the same four-layer structure:[^9][^1]

```
src/
  ServiceName.Domain/          ← Entities, Value Objects, Domain Events, Interfaces
  ServiceName.Application/     ← Commands/Queries (CQRS), DTOs, Validators
  ServiceName.Infrastructure/  ← EF Core, Repos, Encryption, external adapters
  ServiceName.API/             ← Controllers, Middleware, DI wiring
```

Dependency rule: Domain has **zero** external dependencies. Each outer layer depends only inward.

### 3.2 CQRS + MediatR

Operations go through MediatR handlers:[^10][^11]

- **Commands** — mutate state (`CreateClientCommand`, `LockNoteCommand`)
- **Queries** — read-only (`GetClientByIdQuery`, `ListAppointmentsQuery`)

Three **Pipeline Behaviors** cover all cross-cutting concerns — nothing more needed:

| Behavior | Responsibility |
|---|---|
| `ValidationBehavior<TReq, TRes>` | FluentValidation before handler runs |
| `LoggingBehavior<TReq, TRes>` | Structured logging with correlation ID |
| `ExceptionBehavior<TReq, TRes>` | Maps exceptions to `ProblemDetails` responses |

> **Audit** is handled explicitly inside Command handlers that mutate PHI — not as a blanket pipeline behavior on every request. This keeps audit writes intentional and meaningful.

### 3.3 DDD Tactics (lightweight)

Only what adds real value:[^2][^1]

- **Aggregate Roots** — enforce invariants, private constructors + factory methods
- **Value Objects** — for types that need validation and equality by value (`Email`, `PhoneNumber`, `NoteContent`)
- **Repository pattern** — one interface per aggregate in Domain, EF Core implementation in Infrastructure
- **Domain Events** — raised inside aggregates, dispatched synchronously via MediatR `Publish` after `SaveChangesAsync`

No Outbox, no message broker — domain events are in-process only. Cross-service side effects happen via direct HTTP calls in Application layer handlers.

### 3.4 Security Patterns

| Control | Implementation |
|---|---|
| JWT validation | `AddJwtBearer` → Keycloak JWKS auto-discovery |
| RBAC | `[Authorize(Policy)]` mapped to Keycloak role claims |
| Multi-tenancy isolation | EF Core `HasQueryFilter(e => e.TenantId == _currentTenant.Id)` |
| PHI encryption at rest | `EncryptedStringConverter` (AES-256-GCM) on sensitive EF Core columns |
| File encryption | AES-256-GCM, per-tenant key from env config, streaming-only download |
| Input validation | FluentValidation in pipeline — rejects bad input before it hits the domain |
| Error safety | `ExceptionBehavior` — no stack traces or internals leak to responses |
| Soft delete | `DeletedAt` / `DeletedBy` on all client data — supports GDPR right to erasure |

***

## 4. Services & Entities

### 4.1 ClientService

**Bounded context:** Client (patient) records — core PHI.

```csharp
// Aggregate Root
class Client
{
    Guid Id
    Guid TenantId
    FullName Name               // Value Object
    DateTime DateOfBirth
    Email? Email                // Value Object
    PhoneNumber? Phone          // Value Object
    string? EmergencyContact
    string? ReferralSource
    ClientStatus Status         // Active | Inactive | Discharged
    RiskLevel RiskLevel         // Low | Medium | High | Crisis
    IReadOnlyList<Diagnosis> Diagnoses
    DateTime CreatedAt
    DateTime? DeletedAt
}

// Owned Entity
class Diagnosis
{
    string IcdCode              // e.g. F32.1
    string Description
    DateTime DiagnosedAt
    Guid DiagnosedByUserId
}
```

**Commands:** `CreateClientCommand`, `UpdateClientCommand`, `DischargeClientCommand`, `AddDiagnosisCommand`, `SetRiskLevelCommand`

**Queries:** `GetClientQuery`, `SearchClientsQuery`, `ListActiveClientsQuery`

***

### 4.2 ScheduleService

**Bounded context:** Appointments and therapist availability.

```csharp
// Aggregate Root
class Appointment
{
    Guid Id
    Guid TenantId
    Guid ClientId
    Guid TherapistId            // Keycloak sub
    DateTime StartsAt
    DateTime EndsAt
    AppointmentType Type        // Initial | Regular | Supervision
    AppointmentStatus Status    // Scheduled | Confirmed | Completed | Cancelled | NoShow
    SessionFormat Format        // InPerson | Telehealth | Phone
    string? CancellationReason
    DateTime CreatedAt
}

// Aggregate Root
class AvailabilityTemplate
{
    Guid Id
    Guid TenantId
    Guid TherapistId
    DayOfWeek Day
    TimeOnly StartTime
    TimeOnly EndTime
    bool IsActive
}
```

**Commands:** `BookAppointmentCommand`, `CancelAppointmentCommand`, `CompleteAppointmentCommand`, `SetAvailabilityCommand`

**Queries:** `GetAppointmentQuery`, `ListUpcomingAppointmentsQuery`, `GetAvailableSlotsQuery`

***

### 4.3 NotesService

**Bounded context:** Clinical documentation — session notes and treatment plans.

```csharp
// Aggregate Root
class SessionNote
{
    Guid Id
    Guid TenantId
    Guid AppointmentId
    Guid ClientId
    Guid AuthorId               // Keycloak sub
    NoteTemplate Template       // SOAP | DAP | BIRP | Free
    NoteContent Content         // Value Object — AES-256-GCM encrypted
    NoteStatus Status           // Draft | Signed | Locked
    DateTime? SignedAt
    Guid? SignedByUserId
    DateTime? LockedAt
    IReadOnlyList<NoteAddendum> Addenda
    DateTime CreatedAt
}

// Owned Entity
class NoteAddendum
{
    Guid Id
    NoteContent Content         // encrypted
    Guid AddedByUserId
    DateTime AddedAt
}

// Aggregate Root
class TreatmentPlan
{
    Guid Id
    Guid TenantId
    Guid ClientId
    Guid TherapistId
    string PresentingProblem
    IReadOnlyList<TreatmentGoal> Goals
    PlanStatus Status           // Draft | Active | Completed | Discontinued
    DateTime StartDate
    DateTime? ReviewDate
    DateTime CreatedAt
}

// Owned Entity
class TreatmentGoal
{
    Guid Id
    string Description
    string Intervention
    GoalStatus Status           // InProgress | Achieved | Discontinued
    int ProgressPercent
}
```

**Commands:** `CreateNoteCommand`, `UpdateNoteDraftCommand`, `SignNoteCommand`, `LockNoteCommand`, `AddAddendumCommand`, `CreateTreatmentPlanCommand`, `UpdateGoalProgressCommand`

**Queries:** `GetNoteQuery`, `ListClientNotesQuery`, `GetTreatmentPlanQuery`

***

### 4.4 DocumentService

**Bounded context:** Encrypted file storage for intake forms, consents, assessments.

```csharp
// Aggregate Root
class Document
{
    Guid Id
    Guid TenantId
    Guid? ClientId
    Guid UploadedByUserId       // Keycloak sub
    string FileName
    string ContentType
    long FileSizeBytes
    string StoragePath          // Internal path — never exposed in responses
    DocumentCategory Category   // Intake | ConsentForm | Assessment | Other
    bool IsArchived
    DateTime UploadedAt
    DateTime? ArchivedAt
}
```

**Encryption:** AES-256-GCM, key per tenant stored in environment config. Download is always streamed through the service — no direct file URL ever returned.

**Commands:** `UploadDocumentCommand`, `ArchiveDocumentCommand`, `DeleteDocumentCommand`

**Queries:** `GetDocumentMetadataQuery`, `ListClientDocumentsQuery`, `DownloadDocumentQuery`

***

### 4.5 AuditService

**Bounded context:** Append-only security and access event log.

```csharp
// Aggregate Root — INSERT only, no UPDATE or DELETE ever
class AuditEntry
{
    Guid Id
    Guid TenantId
    Guid? UserId                // Keycloak sub
    string Action               // "client.viewed" | "note.locked" | "login.failed"
    string ResourceType         // "Client" | "Note" | "Document"
    Guid? ResourceId
    string? OldValueJson        // Before-state snapshot (mutations only)
    string? NewValueJson        // After-state snapshot (mutations only)
    string IpAddress
    string CorrelationId
    AuditSeverity Severity      // Info | Warning | Critical
    DateTime OccurredAt
}
```

**Design rules:**
- DB role has `INSERT` only — no `UPDATE` or `DELETE` granted to the application user
- Write endpoint is **internal only** — blocked by YARP from public access
- Other services call AuditService via HTTP in their command handlers (PHI mutations only, not reads)
- Admin query endpoint for compliance review

***

### 4.6 API Gateway (YARP)

Reverse proxy — not a microservice:[^3][^4]

- Routes by path prefix to upstream services
- Validates Keycloak JWT (RS256 via JWKS auto-discovery) on every request
- Rate limiting per IP (`Microsoft.AspNetCore.RateLimiting`)
- Injects `X-Correlation-Id` header on all forwarded requests
- Blocks internal write endpoints (AuditService) from public access
- Returns unified `ProblemDetails` on auth/routing errors
- **Tenant provisioning middleware:** on first JWT arrival, if `tenant_id` not in app DB → inserts a `TenantProfile` row (shared DB table read by all services) — no separate service needed

***

## 5. Shared Kernel

A `Sophrosync.SharedKernel` project referenced by all services:[^1][^9]

```
SharedKernel/
  Abstractions/
    IAggregateRoot.cs
    IRepository<T>.cs
    IDomainEvent.cs
    ICurrentTenant.cs           // Resolved from "tenant_id" JWT claim
    ICurrentUser.cs             // Resolved from "sub" JWT claim
  Domain/
    Entity.cs                   // Base: Guid Id, CreatedAt
    AggregateRoot.cs            // + RaiseDomainEvent(), ClearEvents()
    ValueObject.cs              // Equality by value
  Behaviors/
    ValidationBehavior.cs
    LoggingBehavior.cs
    ExceptionBehavior.cs
  Models/
    PaginatedList<T>.cs
  Security/
    EncryptedStringConverter.cs // EF Core AES-256-GCM value converter
    TenantQueryExtensions.cs    // Global query filter helper
    KeycloakClaimsExtensions.cs // Extract tenant_id, roles from ClaimsPrincipal
```

***

## 6. Database Layout

Each service owns its own PostgreSQL database. Cross-service joins are **forbidden** — data is fetched via API calls only.[^2]

| Service | Database | Key tables |
|---|---|---|
| Gateway | `sophrosync_tenants` | `tenant_profiles` (shared read, gateway-provisioned) |
| ClientService | `sophrosync_clients` | `clients`, `diagnoses` |
| ScheduleService | `sophrosync_schedule` | `appointments`, `availability_templates` |
| NotesService | `sophrosync_notes` | `session_notes`, `note_addenda`, `treatment_plans`, `treatment_goals` |
| DocumentService | `sophrosync_documents` | `documents` |
| AuditService | `sophrosync_audit` | `audit_entries` |
| Keycloak | `keycloak` | Keycloak-managed |

All app tables: `tenant_id UUID NOT NULL` + EF Core global query filter.

***

## 7. Security Architecture

### 7.1 Controls mapped to OWASP Top 10

| OWASP Risk | Threat | Control | Where |
|---|---|---|---|
| A01 Broken Access Control | Cross-tenant data read | EF Core Global Query Filter on `tenant_id` | Every service |
| A01 Broken Access Control | Endpoint above role | RBAC Policies + `[Authorize]` | Every controller |
| A02 Cryptographic Failures | DB dump exposes PHI | AES-256-GCM column encryption | EF Core ValueConverter |
| A02 Cryptographic Failures | File storage exposure | AES-256-GCM file encryption, streaming only | DocumentService |
| A02 Cryptographic Failures | Token sniffing | TLS 1.2+ | Nginx / Kestrel |
| A03 Injection | SQL injection | EF Core parameterized queries + FluentValidation | All services |
| A07 Auth Failures | Brute-force, session theft | Keycloak lockout + YARP rate limiter | Keycloak + Gateway |
| A09 Logging Failures | No breach evidence | Append-only audit log, INSERT-only DB role | AuditService |

### 7.2 GDPR Coverage (EU scope)

| GDPR Article | Requirement | Implementation |
|---|---|---|
| Art. 25 — Privacy by Design | Default private, minimum data | `tenant_id` filter on by default; no analytics data collected |
| Art. 32 — Security of Processing | Encryption + access control + audit | AES-256-GCM at rest, TLS in transit, RBAC, audit log |
| Art. 17 — Right to Erasure | Delete client data on request | Soft delete + hard delete endpoint in ClientService |
| Art. 30 — Records of Processing | Log who accessed what | `audit_entries` with UserId, Action, ResourceId, OccurredAt |

### 7.3 Deliberately Out of Scope

| Feature | Reason |
|---|---|
| WAF | Infrastructure concern — noted as prod recommendation in thesis |
| Secrets manager (Vault, Key Vault) | Env vars sufficient for diploma; noted as prod gap |
| mTLS between services | Internal Docker network is trusted; cert management adds no demo value |
| Field-level access control (FGA) | 4 Keycloak roles + tenant filter is the right granularity |
| Key rotation | Encryption at rest covers GDPR Art. 32; rotation is a prod ops concern |

***

## 8. Project Structure

```
Sophrosync.sln
  src/
    Gateway/
      Sophrosync.Gateway/
    Services/
      ClientService/
        Sophrosync.Clients.Domain/
        Sophrosync.Clients.Application/
        Sophrosync.Clients.Infrastructure/
        Sophrosync.Clients.API/
      ScheduleService/  ...
      NotesService/     ...
      DocumentService/  ...
      AuditService/     ...
    Shared/
      Sophrosync.SharedKernel/
  tests/
    Sophrosync.Clients.UnitTests/
    Sophrosync.Clients.IntegrationTests/
    ...
  docker/
    docker-compose.yml
    keycloak/
      realm-export.json          ← imported on first Keycloak start
  docs/
    adr-001-architectural-decisions.md
```

***

## 9. Dependencies (NuGet)

| Package | Purpose |
|---|---|
| `MediatR` | CQRS handlers + pipeline behaviors |
| `FluentValidation.AspNetCore` | Input validation |
| `Microsoft.EntityFrameworkCore` + `Npgsql.EntityFrameworkCore.PostgreSQL` | ORM + PostgreSQL |
| `Yarp.ReverseProxy` | API Gateway |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT resource server (Keycloak tokens) |
| `Keycloak.AuthServices.Authentication` | Keycloak role claim mapping for .NET |
| `Serilog.AspNetCore` | Structured logging with correlation ID |
| `Bogus` | Test data generation |
| `Testcontainers.PostgreSql` | Integration tests against real DB |
| `Testcontainers.Keycloak` | Integration tests against real Keycloak |

---

## References

1. [Clean Architecture and Domain-Driven Design in Practice 2025](https://wojciechowski.app/en/articles/clean-architecture-domain-driven-design-2025) - Complete guide to Clean Architecture and Domain-Driven Design: application layers, Bounded Contexts,...

2. [Microservices Clean Architecture CQRS DDD - .NET 8 Complete ...](https://mohd2sh.github.io/CleanArchitecture-DDD-CQRS-Microservices/) - Complete .NET 8 microservices architecture with Clean Architecture, DDD, and CQRS. Automated archite...

3. [Keycloak Tutorial for .NET Developers - Julio Casal](https://juliocasal.com/blog/keycloak-tutorial-for-net-developers) - So today I'll show you how to secure your ASP.NET Core Apps with OIDC and Keycloak, from scratch. Le...

4. [Comprehensive Guide to Using Keycloak with .NET 8: Compression, Identity Server Comparison, Pros & Cons, and Best Practices](https://www.linkedin.com/pulse/comprehensive-guide-using-keycloak-net-8-compression-identity-ahmed-dxfze) - Introduction Authentication and authorization are critical components of modern applications. Keyclo...

5. [How to Secure Your .NET 8 and Angular Apps with Keycloak](https://saas101.tech/modern-authentication-in-2026-how-to-secure-your-net-8-and-angular-apps-with-keycloak/amp/) - Modern Authentication in 2026: How to Secure Your .NET 8 and Angular Apps with Keycloak Sas 101 %

6. [Multi-tenant architectures in Keycloak (realms vs clients vs new organizations)](https://www.reddit.com/r/KeyCloak/comments/1nka29r/multitenant_architectures_in_keycloak_realms_vs/) - Multi-tenant architectures in Keycloak (realms vs clients vs new organizations)

7. [Understanding Multi-Tenancy Options in Keycloak - Phase Two](https://phasetwo.io/blog/multi-tenancy-options-keycloak/) - As more companies build SaaS platforms, the need to serve multiple customer groups—or tenants—from a...

8. [Keycloak multi-tenant architecture - Cloud-IAM](https://www.cloud-iam.com/post/keycloak-multi-tenancy/) - In a cloud infrastructure, a multi-tenant architecture means that clients* within the same deploymen...

9. [An Inside Look Into Our .Net Clean Architecture Repo - Dev Blogs](https://devblogs.microsoft.com/ise/next-level-clean-architecture-boilerplate/) - Our boilerplate is designed to kickstart your project with a clean architecture, ensuring maintainab...

10. [Implementing CQRS with MediatR in .NET 8: A Complete Guide](https://dev.to/adrianbailador/implementing-cqrs-with-mediatr-in-net-8-a-complete-guide-1kof) - This diagram shows how write operations (Commands) and read operations (Queries) are handled separat...

11. [CQRS and MediatR in ASP.NET Core - Building Scalable Systems](https://codewithmukesh.com/blog/cqrs-and-mediatr-in-aspnet-core/) - In this article, we will explore this pattern, and use the MediatR package in ASP.NET Core to implem...

