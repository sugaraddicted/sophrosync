# Sophrosync — Services Extension Blueprint

**Version:** 1.0
**Date:** 2026-03-04
**Scope:** NotificationService, ConsentService, ReportingService (Phase 1 — full scaffold); BillingService (Phase 2 — blueprint only)

---

## 1. Gap Analysis Against Architecture Spec v1.2

The original spec defines 5 microservices (Client, Schedule, Notes, Document, Audit) + YARP Gateway + SharedKernel. The following gaps were identified:

| Gap | Impact | Phase |
|-----|--------|-------|
| **NotificationService** | No appointment reminders, no high-risk alerts, no note-signing nudges — platform is unusable without async user communication | 1 — Scaffolded |
| **ConsentService** | GDPR Art. 7 (explicit consent) is referenced in spec but has no service — direct compliance gap | 1 — Scaffolded |
| **ReportingService** | No clinical outcomes, no GDPR Art. 30 RoPA, no practice analytics — core SaaS feature missing | 1 — Scaffolded |
| **BillingService** | Explicitly deferred in spec — needed for SaaS monetisation | 2 — Blueprint only |

---

## 2. SharedKernel Additions

Two new files added to `Sophrosync.SharedKernel` beyond the original spec:

| File | Purpose |
|------|---------|
| `Http/ResiliencePolicy.cs` | Shared Polly retry (3 attempts, exponential back-off) + circuit breaker (5 failures → 30s open) factory |
| `Http/ITypedServiceClient.cs` | Marker interface for all typed HTTP inter-service clients |

`Polly` and `Microsoft.Extensions.Http.Polly` are promoted to SharedKernel standard dependencies.

---

## 3. NotificationService

### 3.1 Overview

Delivers in-app and email notifications to therapists and admins. Called by other services via `POST /internal/notifications/send`.

**Database:** `sophrosync_notifications`

### 3.2 Domain Model

| Aggregate | Key Properties | Notes |
|-----------|---------------|-------|
| `Notification` | Channel, Type, encrypted Body, Status state machine, RetryCount, ScheduledFor, CorrelationId | Soft delete; idempotent on CorrelationId |
| `NotificationPreference` | UserId (null = tenant default), PreferredChannel, EmailEnabled, InAppEnabled | Per-user or tenant-level defaults |

**Status state machine:** `Pending → Sent | Failed | Dismissed | Retrying`

### 3.3 CQRS Operations

**Commands:**
- `SendNotificationCommand` — idempotent via CorrelationId; returns NotificationId
- `DismissNotificationCommand` — marks notification dismissed
- `RetryFailedNotificationCommand` — increments retry counter
- `UpdateNotificationPreferenceCommand` — upserts per-user preferences
- `SetTenantNotificationDefaultsCommand` — upserts tenant-level defaults (admin only)

**Queries:**
- `GetInboxQuery` — paginated user inbox
- `GetUnreadCountQuery` — unread count for badge display
- `GetNotificationByIdQuery` — single notification detail
- `ListSentNotificationsQuery` — admin/supervisor sent log
- `GetNotificationPreferenceQuery` — user preference fetch

### 3.4 Background Processing

`NotificationDispatcherService : BackgroundService` polls `notifications` table every **60 seconds** for records where `Status = Pending AND ScheduledFor ≤ NOW()`. Uses Polly retry with 3 in-process attempts and exponential back-off per notification.

Channel abstraction:
- `INotificationChannel` — dispatches by `NotificationChannel` enum
- `EmailNotificationChannel` — MailKit + MimeKit SMTP send
- `InAppNotificationChannel` — no-op (DB record IS the in-app notification)

### 3.5 Security

- Body column: AES-256-GCM via `EncryptedStringConverter`
- Internal send endpoint blocked from public access by YARP
- EF Core global query filter: `tenant_id` + `deleted_at IS NULL`

### 3.6 New NuGet Packages

`MailKit 4.8.0`, `MimeKit 4.8.0`, `Polly 8.4.2`, `Microsoft.Extensions.Http.Polly 8.0.0`

---

## 4. ConsentService

### 4.1 Overview

Manages GDPR Art. 7 explicit consent lifecycle: versioned templates → issued requests → append-only records. Provides a cached status check endpoint consumed by other services.

**Database:** `sophrosync_consent`

### 4.2 Domain Model

| Aggregate | Key Properties | Notes |
|-----------|---------------|-------|
| `ConsentTemplate` | Purpose, Title, BodyText, VersionNumber, Status | Immutable once Published; Draft → Published → Retired |
| `ConsentRequest` | ClientId, TemplateId, Status, ExpiresAt (TTL) | Pending → Completed \| Expired \| Revoked |
| `ConsentRecord` | Action (Granted\|Withdrawn), encrypted IpAddress, encrypted TemplateBodySnapshot | **Append-only** — no UPDATE or DELETE ever; GDPR Art. 7 proof |

### 4.3 Append-Only Consent Records

`consent_records` table: INSERT + SELECT only. Application-level enforcement: `ConsentRecordRepository.Update()` and `.Remove()` throw `InvalidOperationException`. DB-level: the `sophrosync` app user should be granted only INSERT + SELECT on this table in production.

### 4.4 CQRS Operations

**Commands:**
- `CreateConsentTemplateCommand` — creates draft template
- `PublishConsentTemplateCommand` — makes template live; raises `ConsentTemplatePublishedDomainEvent`
- `RetireConsentTemplateCommand` — retires published template
- `IssueConsentRequestCommand` — issues request to client with TTL
- `RecordConsentGrantedCommand` — records grant, completes request, invalidates cache
- `WithdrawConsentCommand` — records withdrawal, invalidates cache
- `RevokeConsentRequestCommand` — admin emergency revoke
- `ExpireOverdueRequestsCommand` — batch expire (called by background service)

**Queries:**
- `GetConsentStatusQuery` — **60s IMemoryCache TTL**, keyed on `(tenantId, clientId, purpose)` — used by all other services
- `ListClientConsentHistoryQuery` — full audit trail per client
- `ListPendingConsentRequestsQuery` — pending requests per client
- `GetConsentAuditSummaryQuery` — full audit export (admin only)
- `GetConsentTemplateQuery` / `ListConsentTemplatesQuery` — template management

### 4.5 Background Processing

`ConsentExpiryService : BackgroundService` runs every **24 hours**, dispatching `ExpireOverdueRequestsCommand` via scoped MediatR. Expired requests raise `ConsentRequestExpiredDomainEvent`.

### 4.6 Inter-Service API

`GET /internal/consent/status?tenantId=&clientId=&purpose=` — 60s TTL cache per `(tenantId, clientId, purpose)` tuple.

### 4.7 New NuGet Packages

`Polly 8.4.2`, `Microsoft.Extensions.Caching.Memory 8.0.1`

---

## 5. ReportingService

### 5.1 Overview

Aggregates data from other services to produce clinical outcome reports, practice analytics, GDPR RoPA, and ad-hoc summaries. Reports run asynchronously (202 Accepted + RunId pattern).

**Database:** `sophrosync_reporting`

### 5.2 Domain Model

| Aggregate | Key Properties | Notes |
|-----------|---------------|-------|
| `ReportDefinition` | Type, Format, Schedule VO, IsActive | Owns `ReportSchedule` value object (stored as owned entity) |
| `ReportRun` | Status, encrypted ResultJson, PeriodStart/End, DeletedAt | `DeleteResult()` nulls ResultJson for GDPR data minimisation |

**Read models (in-memory, not persisted):** `ClinicalOutcomeSummaryDto`, `PracticeAnalyticsSummaryDto`, `GdprRoPAEntryDto[]`

### 5.3 CQRS Operations

**Commands:**
- `CreateReportDefinitionCommand` — creates report definition
- `TriggerReportRunCommand` — creates queued run, returns 202 + RunId
- `GenerateScheduledReportsCommand` — bulk trigger for due scheduled definitions
- `DeleteReportRunCommand` — nulls ResultJson (GDPR data minimisation, keeps metadata)

**Queries:**
- `GetReportRunQuery` — run status + result
- `GetClinicalOutcomeSummaryQuery` — 5-min cache; calls ClientService
- `GetPracticeAnalyticsQuery` — 5-min cache; calls ScheduleService
- `GetGdprRoPAQuery` — static config-driven, no PHI, returns `GdprRoPAConfiguration.GetEntries()`
- `GetAppointmentSummaryQuery` — calls ScheduleService
- `GetNoteCompletionRateQuery` — calls NotesService

### 5.4 Typed HTTP Clients

All four clients use `ResiliencePolicy.GetRetryWithCircuitBreaker()` (3 retries + circuit breaker):

| Client | Calls | Endpoint |
|--------|-------|----------|
| `IClientServiceClient` | ClientService | `GET /internal/clients/summary` |
| `IScheduleServiceClient` | ScheduleService | `GET /internal/appointments/summary` |
| `INotesServiceClient` | NotesService | `GET /internal/notes/summary` |
| `IConsentServiceClient` | ConsentService | `GET /internal/consent/status` |

### 5.5 Background Processing

`ReportSchedulerService : BackgroundService` polls every **24 hours**, dispatching `GenerateScheduledReportsCommand`. Definitions are considered due if `LastRunAt` is null or > 23 hours ago.

### 5.6 Role Scoping

- `therapist` — own-client data only (enforced by EF Core global query filter + `ICurrentUser`)
- `admin` / `supervisor` — full tenant data

### 5.7 Required New Endpoints on Existing Services

| Service | Controller | Endpoint | Status |
|---------|-----------|---------|--------|
| `Sophrosync.Clients.API` | `InternalClientsController` | `GET /internal/clients/summary` | Stub created |
| `Sophrosync.Schedule.API` | `InternalAppointmentsController` | `GET /internal/appointments/summary` | Stub created |
| `Sophrosync.Notes.API` | `InternalNotesController` | `GET /internal/notes/summary` | Stub created |

### 5.8 New NuGet Packages

`Polly 8.4.2`, `Microsoft.Extensions.Http.Polly 8.0.0`, `Microsoft.Extensions.Caching.Memory 8.0.1`, `CsvHelper 33.0.1`, `QuestPDF 2024.10.5`

---

## 6. BillingService — Phase 2 Blueprint

> **No code created. Blueprint documented here for Phase 2 planning.**

### 6.1 Overview

Handles practice subscriptions, invoicing, and Stripe payment processing. Deliberately deferred from MVP.

### 6.2 Domain Model

| Aggregate | Key Properties |
|-----------|---------------|
| `TenantSubscription` | TenantId, Plan (Free\|Pro\|Enterprise), Status (Active\|PastDue\|Cancelled\|GracePeriod), StripeCustomerId, CurrentPeriodEnd |
| `Invoice` | TenantId, StripeInvoiceId, Amount, Currency, Status (Draft\|Sent\|Paid\|Void), DueDate |
| `PaymentMethod` | TenantId, StripePaymentMethodId, Type (Card\|SEPA), IsDefault, ExpiresAt |

### 6.3 Key Patterns

**State machine:** `Active → PastDue → GracePeriod (7 days) → Cancelled`

**Stripe webhook handler:**
- Idempotent via `StripeEventId` unique constraint
- Events: `invoice.payment_succeeded`, `invoice.payment_failed`, `customer.subscription.deleted`
- Webhook signature verification with `Stripe.net`

**Commands:** `CreateSubscriptionCommand`, `UpdateSubscriptionPlanCommand`, `CancelSubscriptionCommand`, `ProcessWebhookCommand` (idempotent), `IssueInvoiceCommand`

**Queries:** `GetSubscriptionStatusQuery`, `GetInvoiceHistoryQuery`, `GetUsageSummaryQuery`

### 6.4 YARP Routes (Phase 2 — add when service exists)

```json
"billing-route": {
  "ClusterId": "billing-cluster",
  "Match": { "Path": "/api/billing/{**catch-all}" },
  "AuthorizationPolicy": "default"
},
"internal-billing-route": {
  "ClusterId": "billing-cluster",
  "Match": { "Path": "/internal/billing/{**catch-all}" },
  "AuthorizationPolicy": "default",
  "Metadata": { "internal": "true" }
}
```

### 6.5 New NuGet Packages

`Stripe.net`

---

## 7. Database Summary

| Service | Database | Tables |
|---------|----------|--------|
| NotificationService | `sophrosync_notifications` | `notifications`, `notification_preferences` |
| ConsentService | `sophrosync_consent` | `consent_templates`, `consent_requests`, `consent_records` |
| ReportingService | `sophrosync_reporting` | `report_definitions`, `report_runs` |
| BillingService (Phase 2) | `sophrosync_billing` | `tenant_subscriptions`, `invoices`, `payment_methods` |

---

## 8. GDPR Compliance Coverage

| GDPR Article | Requirement | Implementation |
|--------------|-------------|----------------|
| Art. 7 — Conditions for consent | Explicit consent with proof | ConsentService append-only records with encrypted IP + template snapshot |
| Art. 30 — Records of processing | RoPA documentation | `GdprRoPAConfiguration` static class in ReportingService (6 processing activities) |
| Art. 17 — Right to erasure | Delete PHI on request | `DeleteReportRunCommand` nulls ResultJson; NotificationService soft delete |
| Art. 25 — Privacy by design | Data minimisation | ReportRun result payload deletable; read models are not persisted to DB |
| Art. 32 — Security | Encryption + access control | AES-256-GCM on sensitive columns, INSERT-only DB role for `consent_records` |

---

## 9. Verification Checklist

- [ ] `dotnet build Sophrosynс.sln` — all 17 projects build without errors
- [ ] `dotnet ef migrations add InitialCreate` succeeds for NotificationsDbContext, ConsentDbContext, ReportingDbContext
- [ ] ConsentService: `consent_records` DB role restricted to INSERT + SELECT in production
- [ ] Gateway: internal routes (`/internal/**`) blocked from public clients
- [ ] All service `appsettings.json` have correct Keycloak authority configured
- [ ] Polly retry + circuit breaker applied to all 4 ReportingService HTTP clients
- [ ] `GdprRoPAConfiguration.GetEntries()` returns 6 processing activity entries
- [ ] `NotificationDispatcherService` poll interval: 60 seconds
- [ ] `ConsentExpiryService` + `ReportSchedulerService` poll interval: 24 hours
