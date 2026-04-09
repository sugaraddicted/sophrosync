# Notes Feature — Implementation Plan

**Version:** 1.1
**Date:** 2026-03-19
**Scope:** Full-stack Notes feature (NotesService backend + Angular SPA frontend)

---

## 1. Competitive Analysis — What Similar Apps Provide

### SimplePractice
- Progress notes linked to sessions with a sign/lock workflow (Draft → Locked → Amended)
- Supervisor co-signing flow before a note can be finalized
- Customizable note templates per practice
- Late-entry flagging when a note is created more than 24 h after the session
- PDF export of signed notes with therapist signature, credentials, and timestamp

### TherapyNotes
- Modality-aware note types: Individual, Group, Couples, Family
- DSM-5/ICD-10 diagnosis codes embedded in the note
- Structured SOAP (Subjective / Objective / Assessment / Plan) layout
- Notes always linked to an appointment; no orphan notes
- Amendment creates a new record; original is locked and displayed as "amended by"

### Theranest / TheraNinja
- Quick progress notes with a rich-text body + structured fields (interventions, client response, plan)
- Custom field sets per note type (configurable per practice)
- Batch signing for high-volume practices
- In-note medication tracking (out of scope for our MVP)

### What We Will Build (MVP Scope)
Therapist-facing clinical notes with:
- Six note types; DAP and SOAP prioritized as the most common in private practice
- **Sign and Lock are distinct actions** — Sign = professional attestation; Lock = permanent immutability (conflating them is SimplePractice's mistake)
- Amendment trail (addenda only once locked — legal requirement)
- PHI-encrypted content stored in a dedicated Postgres database
- Full tenant isolation and RBAC aligned with the rest of the platform
- Author name denormalized at write time so historical attribution survives account changes

---

## 2. Domain Model

### 2.1 `Note` Aggregate Root

```
Note
├── Id                 Guid              (PK)
├── TenantId           Guid              (multi-tenancy)
├── ClientId           Guid              (cross-service ref — no FK, validated at app layer)
├── AppointmentId      Guid?             (optional link to ScheduleService)
├── TherapistId        Guid              (authoring user's sub claim)
├── AuthorFullName     string            (denormalized at create time — survives account renames/deactivation)
├── Type               string            (NoteType constant)
├── Title              string [ENCRYPTED] (PHI — optional display label)
├── Content            string [ENCRYPTED] (PHI — main clinical body)
├── Tags               string            (comma-separated labels, NOT PHI, max 500 chars)
├── Status             string            (NoteStatus constant)
├── SignedAt           DateTime?         (UTC — when therapist attested)
├── SignedByUserId     Guid?             (who signed)
├── SignedByFullName   string?           (denormalized at sign time)
├── LockedAt           DateTime?         (UTC — when note became permanently immutable)
├── LockedByUserId     Guid?             (who locked — may differ from signer in co-sign flow)
├── LockedByFullName   string?           (denormalized at lock time)
├── AmendedFromId      Guid?             (non-null when this note amends a previous one)
├── IsDeleted          bool              (soft delete — Draft notes only)
├── DeletedAt          DateTime?
├── CreatedAt          DateTime          (from AggregateRoot)
└── UpdatedAt          DateTime          (from AggregateRoot)
```

**PHI fields:** `Title`, `Content` — both use `EncryptedStringConverter` (AES-256-GCM) via EF Core value converter, exactly as in ClientService.

**Denormalized name fields** (`AuthorFullName`, `SignedByFullName`, `LockedByFullName`): plain text, populated from `ICurrentUser.FullName` at the time of the action. Not encrypted — therapist names are not client PHI. Never updated after the fact.

### 2.2 Note Types (`NoteType` constants)

Listed in MVP priority order — all six live in the domain from day one; template UI is built in priority order.

| Constant       | Display Name        | Typical Use                                     | MVP Priority |
|----------------|---------------------|-------------------------------------------------|--------------|
| `DAP`          | DAP Note            | Data / Assessment / Plan — most common in private practice | P1 |
| `SOAP`         | SOAP Note           | Subjective / Objective / Assessment / Plan      | P1           |
| `FreeForm`     | Free Form           | Specialized modalities (EMDR, somatic, etc.)    | P1           |
| `Intake`       | Intake / Assessment | First-session biopsychosocial assessment        | P2           |
| `Treatment`    | Treatment Plan      | Goals, interventions, review schedule           | P2           |
| `Discharge`    | Discharge Summary   | End-of-care summary                             | P2           |

### 2.3 Status State Machine

Sign and Lock are **two separate domain actions** with different clinical meanings.

```
         create
           │
           ▼
        [Draft] ──────────────────────────────────────────────┐
           │                                                   │ request co-sign
     self-sign                                                 ▼
           │                                          [PendingCoSign]
           ▼                                                   │
        [Signed] ◄─────────────────────── co-sign approved ───┘
           │
         lock
           │
           ▼
        [Locked]
           │
         amend (creates new Draft with AmendedFromId)
           │
           ▼
        [Amended]   ←── the Locked note after its amendment is created
```

- **Draft** — editable by the authoring therapist. Soft-deletable.
- **PendingCoSign** — read-only for the author; awaiting supervisor action. Not deletable.
- **Signed** — therapist has attested the content. Edits are still technically possible with a logged warning (UI warns; backend allows via `Update()` with explicit `forceOnSigned: true` flag for accidental click recovery only). Not deletable.
- **Locked** — permanently immutable. `Update()` throws unconditionally. No UI unlock path exists anywhere. Any correction goes through Amendment.
- **Amended** — historical snapshot. The `Locked` note transitions here when a new amendment note is created.

**Business rules enforced in the domain:**
- `Update()` throws if `Status == Locked || Status == Amended`
- `RequestCoSign()` throws if `Status != Draft`
- `Sign()` throws if `Status not in {Draft, PendingCoSign}`
- `Lock()` throws if `Status != Signed`; records `LockedAt`, `LockedByUserId`, `LockedByFullName`
- `SoftDelete()` throws if `Status != Draft`
- `Amend()` throws if `Status != Locked`; transitions to `Amended`; new Draft created by handler

---

## 3. CQRS Operations

### 3.1 Commands

| Command                   | Auth Role            | Description                                              |
|---------------------------|----------------------|----------------------------------------------------------|
| `CreateNoteCommand`       | `CanCreateNotes`     | Creates a Draft note; TherapistId from `ICurrentUser`    |
| `UpdateNoteCommand`       | `CanCreateNotes`     | Updates Title/Content/Tags; only Draft notes             |
| `RequestCoSignCommand`    | `CanCreateNotes`     | Draft → PendingCoSign; only when supervisor role exists  |
| `SignNoteCommand`         | `CanSignNotes`       | Draft/PendingCoSign → Signed; records SignedBy* fields   |
| `LockNoteCommand`         | `CanSignNotes`       | Signed → Locked; records LockedBy* fields; permanent     |
| `AmendNoteCommand`        | `CanAmendNotes`      | Locked → Amended; creates new Draft with AmendedFromId   |
| `DeleteNoteCommand`       | `CanCreateNotes`     | Soft-delete; only Draft notes                            |

### 3.2 Queries

| Query                        | Auth Role               | Description                                       |
|------------------------------|-------------------------|---------------------------------------------------|
| `GetNoteByIdQuery`           | `CanReadNotes`          | Single note; therapist sees own, supervisor all   |
| `GetNotesByClientIdQuery`    | `CanReadNotes`          | Paginated notes for a client                      |
| `GetNotesQuery`              | `CanReadNotes`          | Paginated list; filterable by type/status/date    |

### 3.3 Internal Endpoint (already stubbed)

`GET /internal/notes/summary?tenantId=&from=&to=`

Returns `NoteCompletionRateSummaryDto` consumed by ReportingService:
```json
{
  "totalNotes": 120,
  "lockedNotes": 98,
  "signedNotes": 5,
  "draftNotes": 10,
  "pendingCoSign": 7,
  "completionRate": 0.817
}
```

Completion rate = `lockedNotes / totalNotes` — only Locked notes are considered fully complete.

---

## 4. Security Model

### 4.1 PHI Encryption (at-rest)

```
Column      Converter                     Key source
──────────────────────────────────────────────────────────────
Title       EncryptedStringConverter      Encryption:NotesKey (appsettings / secret manager)
Content     EncryptedStringConverter      Encryption:NotesKey
```

- AES-256-GCM with per-value nonce (12 bytes, prepended to ciphertext)
- Key **must not** be hardcoded — inject via environment variable or Docker secret
- `NotesEncryptionOptions` singleton (mirrors `ClientsEncryptionOptions` pattern)

### 4.2 Tenant Isolation

EF Core global query filter on `Note`:
```csharp
modelBuilder.Entity<Note>()
    .HasQueryFilter(e => !e.IsDeleted && e.TenantId == currentTenant.Id);
```

`TenantId` is **always set by the handler from `ICurrentTenant`**, never from the request body.

### 4.3 RBAC (Keycloak Roles)

| Role               | Notes Access                                                |
|--------------------|-------------------------------------------------------------|
| `therapist`        | Create/read/sign own notes only (EF filter on `TherapistId`)|
| `supervisor`       | Read/sign all notes in tenant; approve co-sign requests     |
| `admin`            | Read-only access to all notes for compliance/audit purposes |

Keycloak realm roles map to `[Authorize(Roles = "...")]` via `JwtBearerDefaults`. For now follows the same "auth deferred for local dev" pattern as ClientsController, with a `// TODO: [Authorize]` comment.

### 4.4 Immutability & Legal Compliance

- **No hard deletes** on signed or amended notes — ever. `SoftDelete()` is limited to Draft status.
- **Amendment creates a new record** — the original Note row is updated to `Status = Amended` with no content change. This satisfies medical records retention law (immutable audit trail).
- **GDPR Art. 17 (Right to Erasure):** Clinical notes are exempt under Art. 9(2)(h) (healthcare processing) and local health data retention laws. Implement erasure as an `IsAnonymised` flag that nulls `Title`/`Content` (replace ciphertext with a sentinel value) rather than deletion. This is a Phase 2 item.

### 4.5 Access Logging (PHI Audit Trail)

Every `GetNoteById` handler should log: `tenantId`, `userId`, `noteId`, `clientId`, `timestamp` at the `Information` level via Serilog. This provides a HIPAA-style access log without a dedicated audit service.

### 4.6 Injection & Input Validation (OWASP)

FluentValidation rules:
- `Content`: required, max 50,000 characters
- `Title`: optional, max 200 characters
- `Tags`: optional, max 500 characters, regex `^[\w ,\-]+$` (no HTML)
- `ClientId`: required, must be a valid non-empty GUID
- `Type`: must be one of the five `NoteType` constants

Content is stored as plaintext (pre-encryption) — **no HTML rendering in the backend**. The Angular front-end must escape or sanitise if rendering in an HTML context.

---

## 5. Project Structure

Following the exact 4-layer pattern established by ClientService:

```
src/Services/NotesService/
├── Sophrosync.Notes.Domain/
│   ├── Sophrosync.Notes.Domain.csproj
│   ├── Entities/
│   │   ├── Note.cs                  ← aggregate root
│   │   ├── NoteType.cs              ← string constants
│   │   └── NoteStatus.cs            ← string constants + state machine helpers
│   └── Interfaces/
│       └── INoteRepository.cs
│
├── Sophrosync.Notes.Application/
│   ├── Sophrosync.Notes.Application.csproj
│   ├── DTOs/
│   │   └── NoteDto.cs
│   ├── Commands/
│   │   ├── CreateNote/
│   │   │   ├── CreateNoteCommand.cs
│   │   │   ├── CreateNoteCommandHandler.cs
│   │   │   └── CreateNoteCommandValidator.cs
│   │   ├── UpdateNote/
│   │   │   ├── UpdateNoteCommand.cs
│   │   │   ├── UpdateNoteCommandHandler.cs
│   │   │   └── UpdateNoteCommandValidator.cs
│   │   ├── SignNote/
│   │   │   ├── SignNoteCommand.cs
│   │   │   └── SignNoteCommandHandler.cs
│   │   ├── LockNote/
│   │   │   ├── LockNoteCommand.cs
│   │   │   └── LockNoteCommandHandler.cs
│   │   ├── RequestCoSign/
│   │   │   ├── RequestCoSignCommand.cs
│   │   │   └── RequestCoSignCommandHandler.cs
│   │   ├── AmendNote/
│   │   │   ├── AmendNoteCommand.cs
│   │   │   ├── AmendNoteCommandHandler.cs
│   │   │   └── AmendNoteCommandValidator.cs
│   │   └── DeleteNote/
│   │       ├── DeleteNoteCommand.cs
│   │       └── DeleteNoteCommandHandler.cs
│   └── Queries/
│       ├── GetNoteById/
│       │   ├── GetNoteByIdQuery.cs
│       │   └── GetNoteByIdQueryHandler.cs
│       ├── GetNotesByClientId/
│       │   ├── GetNotesByClientIdQuery.cs
│       │   └── GetNotesByClientIdQueryHandler.cs
│       └── GetNotes/
│           ├── GetNotesQuery.cs
│           └── GetNotesQueryHandler.cs
│
├── Sophrosync.Notes.Infrastructure/
│   ├── Sophrosync.Notes.Infrastructure.csproj
│   ├── Persistence/
│   │   ├── NotesDbContext.cs
│   │   ├── NotesDbContextFactory.cs
│   │   ├── NotesEncryptionOptions.cs
│   │   ├── Configurations/
│   │   │   └── NoteConfiguration.cs
│   │   ├── Migrations/              ← generated by dotnet ef
│   │   └── Repositories/
│   │       └── NoteRepository.cs
│   └── ServiceCollectionExtensions.cs
│
└── Sophrosync.Notes.API/            ← already exists (stub)
    ├── Sophrosync.Notes.API.csproj  ← needs full dependency wiring
    ├── Controllers/
    │   ├── NotesController.cs       ← public CRUD + sign/amend
    │   └── InternalNotesController.cs ← already exists; implement summary endpoint
    └── Program.cs                   ← needs MediatR, EF, SharedKernel wiring
```

---

## 6. Database

**Database name:** `sophrosync_notes`
**Table:** `notes`

```sql
CREATE TABLE notes (
    id                  uuid            PRIMARY KEY,
    tenant_id           uuid            NOT NULL,
    client_id           uuid            NOT NULL,
    appointment_id      uuid,
    therapist_id        uuid            NOT NULL,
    author_full_name    varchar(200)    NOT NULL,  -- denormalized at create time
    type                varchar(20)     NOT NULL,
    title               text,                      -- AES-256-GCM ciphertext (nullable)
    content             text            NOT NULL,  -- AES-256-GCM ciphertext
    tags                varchar(500),
    status              varchar(25)     NOT NULL   DEFAULT 'Draft',
    signed_at           timestamptz,
    signed_by_user_id   uuid,
    signed_by_full_name varchar(200),              -- denormalized at sign time
    locked_at           timestamptz,
    locked_by_user_id   uuid,
    locked_by_full_name varchar(200),              -- denormalized at lock time
    amended_from_id     uuid            REFERENCES notes(id),
    is_deleted          boolean         NOT NULL   DEFAULT false,
    deleted_at          timestamptz,
    created_at          timestamptz     NOT NULL,
    updated_at          timestamptz     NOT NULL
);

-- Indexes
CREATE INDEX ix_notes_tenant_client    ON notes (tenant_id, client_id)    WHERE NOT is_deleted;
CREATE INDEX ix_notes_tenant_therapist ON notes (tenant_id, therapist_id) WHERE NOT is_deleted;
CREATE INDEX ix_notes_tenant_status    ON notes (tenant_id, status)       WHERE NOT is_deleted;
```

Migration generated via `dotnet ef migrations add InitialCreate --project Sophrosync.Notes.Infrastructure --startup-project Sophrosync.Notes.API`.

---

## 7. API Endpoints

All public routes are proxied through the YARP Gateway at `/api/notes/**`.

### NotesController — `[Route("api/notes")]`

| Method   | Route                         | Command/Query                 | Returns                    |
|----------|-------------------------------|-------------------------------|----------------------------|
| `GET`    | `/api/notes`                  | `GetNotesQuery`               | `PagedList<NoteDto>`       |
| `GET`    | `/api/notes/{id}`             | `GetNoteByIdQuery`            | `NoteDto` / 404            |
| `GET`    | `/api/notes/client/{clientId}`| `GetNotesByClientIdQuery`     | `PagedList<NoteDto>`       |
| `POST`   | `/api/notes`                  | `CreateNoteCommand`           | 201 + `NoteDto`            |
| `PUT`    | `/api/notes/{id}`             | `UpdateNoteCommand`           | `NoteDto` / 404 / 409      |
| `POST`   | `/api/notes/{id}/sign`           | `SignNoteCommand`          | `NoteDto` / 404 / 409      |
| `POST`   | `/api/notes/{id}/lock`           | `LockNoteCommand`          | `NoteDto` / 404 / 409      |
| `POST`   | `/api/notes/{id}/request-cosign` | `RequestCoSignCommand`     | `NoteDto` / 404 / 409      |
| `POST`   | `/api/notes/{id}/amend`          | `AmendNoteCommand`         | 201 + new `NoteDto`        |
| `DELETE` | `/api/notes/{id}`             | `DeleteNoteCommand`           | 204 / 404 / 409            |

**409 Conflict** is returned when a state machine transition is invalid (e.g. trying to update a Signed note).

### InternalNotesController — `[Route("internal/notes")]`

| Method | Route                    | Description                                      |
|--------|--------------------------|--------------------------------------------------|
| `GET`  | `/internal/notes/summary`| NoteCompletionRateSummaryDto (for ReportingService) |

---

## 8. NoteDto

```csharp
public sealed record NoteDto(
    Guid Id,
    Guid ClientId,
    Guid? AppointmentId,
    Guid TherapistId,
    string AuthorFullName,
    string Type,
    string? Title,
    string Content,
    string? Tags,
    string Status,
    DateTime? SignedAt,
    Guid? SignedByUserId,
    string? SignedByFullName,
    DateTime? LockedAt,
    Guid? LockedByUserId,
    string? LockedByFullName,
    Guid? AmendedFromId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
```

---

## 9. YARP Gateway Integration

Add to `src/Gateway/Sophrosync.Gateway/appsettings.json`:

```json
"notes-route": {
  "ClusterId": "notes-cluster",
  "Match": { "Path": "/api/notes/{**catch-all}" },
  "AuthorizationPolicy": "default"
},
"internal-notes-route": {
  "ClusterId": "notes-cluster",
  "Match": { "Path": "/internal/notes/{**catch-all}" },
  "AuthorizationPolicy": "default",
  "Metadata": { "internal": "true" }
}
```

```json
"notes-cluster": {
  "Destinations": {
    "notes": { "Address": "http://localhost:5003" }
  }
}
```

---

## 10. Angular SPA Components

### Feature Module: `notes`

```
src/app/features/notes/
├── notes.routes.ts              ← lazy-loaded
├── notes.service.ts             ← HttpClient wrapper for /api/notes
├── components/
│   ├── note-list/               ← paginated, filterable by client/type/status/date
│   ├── note-detail/             ← read-only view with status badge + amendment chain
│   ├── note-form/               ← create + edit Draft notes (rich-text or textarea)
│   ├── note-sign-dialog/        ← confirmation dialog for signing / co-signing
│   └── note-amend-dialog/       ← starts an amendment (copies content to new Draft)
└── models/
    └── note.model.ts
```

### Integration Points
- **Client detail page** — embed a `NoteListComponent` filtered by `clientId`
- **Dashboard** — "Unsigned notes" widget showing count of Draft/PendingCoSign notes for the logged-in therapist
- **Schedule/Calendar** — "Add Note" button on each appointment card, pre-populating `appointmentId`

---

## 11. Implementation Phases

### Phase 1 — Backend Core (NotesService full scaffold)
1. Create `Sophrosync.Notes.Domain` project: `Note`, `NoteType`, `NoteStatus`, `INoteRepository`
2. Create `Sophrosync.Notes.Application` project: all Commands + Queries + DTOs + Validators
3. Create `Sophrosync.Notes.Infrastructure` project: `NotesDbContext`, `NoteConfiguration`, `NoteRepository`, `NotesEncryptionOptions`, `ServiceCollectionExtensions`
4. Wire `Sophrosync.Notes.API` `Program.cs`: MediatR, FluentValidation pipeline, SharedKernel, Infrastructure
5. Implement `NotesController` and complete `InternalNotesController`
6. Add EF Core migration and verify schema against Section 6
7. Add `Encryption:NotesKey` to `appsettings.Development.json` (dev key only; prod via secrets)
8. Register new YARP routes in Gateway
9. Add the three new .csproj files to `Sophrosynс.sln`

### Phase 2 — Security Hardening
1. Add `[Authorize(Roles = "therapist,supervisor,admin")]` to `NotesController`
2. Add therapist-scope EF filter: when role = `therapist`, add `&& e.TherapistId == currentUser.Id` to the global query filter
3. Add PHI access logging in `GetNoteByIdQueryHandler`
4. Write FluentValidation integration tests
5. Tenant isolation unit tests (cross-tenant access attempt returns 0 results)

### Phase 3 — Angular SPA
1. Scaffold `notes` feature module with lazy routes
2. Implement `NoteListComponent` with client-filter and status-filter
3. Implement `NoteFormComponent` for create/edit
4. Implement sign / co-sign dialogs
5. Implement amendment flow UI
6. Embed note list in Client detail page
7. Add "Unsigned notes" widget to Dashboard

### Phase 4 — Post-MVP
- PDF export of locked notes (QuestPDF — already a dependency via ReportingService)
- P2 note type templates: Intake, Treatment Plan, Discharge Summary
- GDPR Art. 17 anonymisation flow for discharged clients (`IsAnonymised` flag, nulls ciphertext)
- Batch locking UI for high-volume therapists (end-of-day workflow)
- Supervisor dashboard: pending co-sign queue

---

## 12. NuGet Packages Required

### Sophrosync.Notes.Domain
- `Sophrosync.SharedKernel` (project reference)

### Sophrosync.Notes.Application
- `MediatR 12.x`
- `FluentValidation 11.x`
- Project refs: Domain, SharedKernel

### Sophrosync.Notes.Infrastructure
- `Microsoft.EntityFrameworkCore 8.x`
- `Npgsql.EntityFrameworkCore.PostgreSQL 8.x`
- `Microsoft.EntityFrameworkCore.Design 8.x`
- Project refs: Application, SharedKernel

### Sophrosync.Notes.API (additions to existing stub)
- `MediatR.Extensions.Microsoft.DependencyInjection` (or bundled with MediatR 12)
- `FluentValidation.AspNetCore 11.x`
- Project refs: Application, Infrastructure

---

## 13. Key Security Decisions Summary

| Decision | Rationale |
|----------|-----------|
| AES-256-GCM on `Title` + `Content` | Both fields contain identifiable clinical information |
| `Tags` NOT encrypted | Tags are non-identifying labels; encryption would prevent server-side filtering |
| `TherapistId` stored as plain GUID | Needed for EF index and filtering; not identifiable without context |
| No hard deletes on any notes | Medical records retention law + GDPR Art. 9(2)(h) exception |
| Amendment requires Locked status | Prevents amendments to non-finalized notes |
| Amendment creates new row | Immutable audit trail; satisfies CQC / state board standards |
| `AuthorFullName` / `SignedByFullName` / `LockedByFullName` denormalized | Attribution must survive Keycloak account renames or deactivation |
| `TenantId` always from `ICurrentTenant` | Prevents tenant spoofing via request body |
| `TherapistId` always from `ICurrentUser` | Prevents therapist spoofing — author is whoever is logged in |
| Internal summary endpoint returns only aggregates | Never exposes raw PHI to inter-service calls |
| No `Unlock` action exists — anywhere | Lock is a legal attestation; reversing it would void the record |
