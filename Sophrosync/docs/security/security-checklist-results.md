# Security Testing Checklist Results

**Date:** 2026-06-05
**Stack:** Sophrosync v0.1 (diploma build — dev branch)
**Tester:** Mariia Prylutska
**Audit method:** Automated code audit (sophrosync-security-test-reviewer agent) + manual code review
**Live scan status:** ZAP / JMeter not yet run — see `run-security-scans.ps1`

---

## Part 1 — Code-Level Security Audit (Static)

### Authentication & Authorization

| Check | Result | Evidence | Notes |
|-------|--------|----------|-------|
| JWT authority + audience configured in Gateway | ✅ PASS | `Gateway/Program.cs:16-17` | RS256 via Keycloak OIDC discovery |
| RS256 algorithm pinned (rejects HS256 / none) | ✅ PASS | `Gateway/Program.cs:21-24` — `ValidAlgorithms = ["RS256"]` | Fixed in this session — was missing |
| RequireHttpsMetadata dev/prod split | ✅ PASS | `Gateway/Program.cs:18` | False in dev, true in prod |
| `GET /api/reports/practice-analytics` requires admin/supervisor | ✅ PASS | `ReportsController.cs:28` — `[Authorize(Roles = "admin,supervisor,practice-admin")]` | Therapist role correctly excluded |
| `/internal/**` paths blocked at Gateway | ✅ PASS | `Gateway/Program.cs:52-60` — 404 middleware runs before YARP | All internal routes unreachable from outside |
| `ClientsController` requires authenticated JWT | ✅ PASS | `ClientsController.cs:17` — `[Authorize]` at class level | Fixed in this session — was missing (dev shortcut) |
| All other public controllers have `[Authorize]` | ✅ PASS | NotesController, ConsentTemplatesController, ConsentRequestsController, AppointmentsController, NotificationsController, ProfileController, ReportsController | Confirmed via grep |
| `RegistrationController` intentionally public | ✅ N/A | No `[Authorize]` is correct — new users must register without a token | |

### Multi-Tenancy Isolation

| Check | Result | Evidence | Notes |
|-------|--------|----------|-------|
| `ClientsDbContext` — TenantId + IsDeleted global filter | ✅ PASS | `ClientsDbContext.cs:51` — `HasQueryFilter(e => !e.IsDeleted && e.TenantId == _currentTenant.Id)` | Both predicates present |
| `NotesDbContext` — TenantId + IsDeleted + therapist scope | ✅ PASS | `NotesDbContext.cs:54-58` — three-predicate filter | Strictest filtering in the codebase |
| `ConsentDbContext` — TenantId on all 4 entities | ✅ PASS | `ConsentDbContext.cs:55-62` | Consent entities use status (Retired/Revoked/Expired) not IsDeleted — by design (GDPR Art.7 immutability) |
| `ScheduleDbContext` — TenantId filter | ✅ PASS | `ScheduleDbContext.cs` | |
| `IdentityDbContext` — TenantId filter | ✅ PASS | `IdentityDbContext.cs` | |
| HKDF per-tenant key derivation | ✅ PASS | `ClientsDbContext.cs`, `NotesDbContext.cs`, `ConsentDbContext.cs` — HKDF-SHA256 at DbContext construction | |
| Security.Tests cross-tenant isolation | ✅ PASS | `Sophrosync.Security.Tests` — 5/5 passing, ~2.6s | Automated regression coverage |

### Input Validation

| Check | Result | Evidence | Notes |
|-------|--------|----------|-------|
| `CreateClientCommand` — Name/Email bounded + email format | ✅ PASS | `CreateClientCommandValidator.cs` — `NotEmpty`, `MaximumLength(200)`, `EmailAddress()` | |
| `UpdateClientCommand` — all fields bounded + status whitelist | ✅ PASS | `UpdateClientCommandValidator.cs` | |
| `CreateNoteCommand` — content/title bounded | ✅ PASS | `CreateNoteCommandValidator.cs` — `MaximumLength(50000)` on content | |
| `IssueConsentRequestCommand` — ExpiresAt in future | ✅ PASS | `IssueConsentRequestCommandValidator.cs` — `GreaterThan(DateTime.UtcNow)` | |
| `RegisterPracticeCommand` — all fields + AcceptedTerms | ✅ PASS | `RegisterPracticeCommandValidator.cs` — `AcceptedTerms == true` enforced | |
| ValidationBehavior in MediatR pipeline | ✅ PASS | `SharedKernel/ValidationBehavior.cs` — wired via AddMediatR in all services | All validators run automatically before handlers |

### PHI Encryption at Rest

| Check | Result | Evidence | Notes |
|-------|--------|----------|-------|
| `Client.Name`, `Email`, `Phone` encrypted | ✅ PASS | `ClientConfiguration.cs:26-37` — `EncryptedStringConverter` on all three | AES-256-GCM |
| `Note.Title`, `Content` encrypted | ✅ PASS | `NoteConfiguration.cs:38-47` | Column sized for base64 expansion |
| `ConsentRecord.IpAddress`, `TemplateBodySnapshot` encrypted | ✅ PASS | `ConsentRecordConfiguration.cs:26-34` | |
| DB stores ciphertext not plaintext | ⏳ PENDING | Requires live DB query + screenshot | Run `SELECT name, email FROM clients LIMIT 1` after stack is up |

### Rate Limiting

| Check | Result | Evidence | Notes |
|-------|--------|----------|-------|
| Gateway rate limiter active | ✅ PASS | `Gateway/Program.cs:29-34` — `PermitLimit=100`, `Window=1min`, `429` on rejection | Partition key: RemoteIpAddress |
| 429 returned on rate limit hit | ✅ PASS | `RejectionStatusCode = StatusCodes.Status429TooManyRequests` | |
| Rate limit under burst test | ⏳ PENDING | Requires JMeter TG2 live run | Lower `PermitLimit` to `10` temporarily for reproducible test |

### GDPR Soft Delete

| Check | Result | Evidence | Notes |
|-------|--------|----------|-------|
| `Client.SoftDelete()` sets `IsDeleted` + `DeletedAt` | ✅ PASS | `Client.cs:62-66` | |
| `Note.SoftDelete()` + full audit trail | ✅ PASS | `Note.cs` — `DeletedAt`, `DeletedByUserId`, `DeletedByFullName` | |
| Soft-deleted records hidden from queries | ✅ PASS | Global query filters in ClientsDbContext + NotesDbContext | EF Core filters applied on all queries |

### Security Response Headers

| Check | Result | Evidence | Notes |
|-------|--------|----------|-------|
| `X-Content-Type-Options: nosniff` | ✅ PASS | `Gateway/Program.cs:64` | Added ec063ba |
| `X-Frame-Options: DENY` | ✅ PASS | `Gateway/Program.cs:65` | Added ec063ba |
| `Referrer-Policy: no-referrer` | ✅ PASS | `Gateway/Program.cs:66` | Added ec063ba |

### Correlation ID

| Check | Result | Evidence | Notes |
|-------|--------|----------|-------|
| `X-Correlation-Id` injected before auth | ✅ PASS | `Gateway/Program.cs:50-54` — middleware runs before UseRateLimiter/UseAuthentication | Fixed in this session — previously after auth, rejected requests had no correlation ID |

---

## Part 2 — Live Stack Checks (Run `.\run-security-scans.ps1`)

### Authentication & Authorization (Live)

| Check | Result | Notes |
|-------|--------|-------|
| `GET /api/clients` without JWT → 401 | ⏳ TODO | |
| `GET /api/clients` with expired JWT → 401 | ⏳ TODO | |
| `GET /api/clients` with valid JWT wrong realm → 401 | ⏳ TODO | |
| `GET /api/reports/practice-analytics` with therapist JWT → 403 | ⏳ TODO | |
| `GET /api/reports/practice-analytics` with admin JWT → 200 | ⏳ TODO | |

### Multi-Tenancy Isolation (Live)

| Check | Result | Notes |
|-------|--------|-------|
| TenantB JWT cannot see TenantA clients | ⏳ TODO | |
| TenantB JWT → `GET /api/clients/{tenantA-id}` → 404 | ⏳ TODO | |

### PHI Encryption at Rest (Live DB)

| Check | Result | Notes |
|-------|--------|-------|
| `SELECT name, email FROM clients LIMIT 1` shows ciphertext | ⏳ TODO | Screenshot for thesis appendix |
| Two tenants, same plaintext name → different ciphertext (HKDF) | ✅ PASS (automated) | Security.Tests `CrossTenantIsolation` covers this |

### Rate Limiting (Live)

| Check | Result | Notes |
|-------|--------|-------|
| 200 rapid requests → 429 observed | ⏳ TODO | Temporarily set `PermitLimit = 10` for test |
| `X-Correlation-Id` present in response headers | ⏳ TODO | |

### GDPR Right to Erasure (Live)

| Check | Result | Notes |
|-------|--------|-------|
| `DELETE /api/clients/{id}` → 204 | ⏳ TODO | |
| `GET /api/clients/{id}` after delete → 404 | ⏳ TODO | |
| Direct DB: row exists with `is_deleted=true` | ⏳ TODO | Screenshot for thesis appendix |

---

## Part 3 — Fixes Applied (This Session)

| Issue | Severity | Fix |
|-------|----------|-----|
| `ClientsController` missing `[Authorize]` — PHI exposed on direct port 5001 | CRITICAL | Added `[Authorize]` at class level |
| JWT RS256 algorithm not pinned — symmetric/none tokens accepted | HIGH | `ValidAlgorithms = new[] { "RS256" }` in Gateway TokenValidationParameters |
| Correlation ID middleware ran after `UseAuthentication` — rejected requests untraced | MEDIUM | Moved correlation ID before `UseRateLimiter` / `UseAuthentication` |
| Missing security response headers | MEDIUM | `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy` (commit ec063ba) |
