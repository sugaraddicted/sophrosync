using Sophrosync.Reporting.Application.DTOs;

namespace Sophrosync.Reporting.Application.Configuration;

/// <summary>
/// Static GDPR Article 30 Record of Processing Activities.
/// Version-controlled here — not DB-driven. No PHI stored.
/// </summary>
public static class GdprRoPAConfiguration
{
    public static IReadOnlyList<GdprRoPAEntryDto> GetEntries() =>
    [
        new GdprRoPAEntryDto(
            ProcessingActivity: "Client Record Management",
            Purpose: "Provision of psychotherapy services; maintaining client care records",
            LegalBasis: "Art. 9(2)(h) GDPR — Healthcare provision",
            DataCategories: "Identity, contact details, health data, session notes, diagnoses",
            DataSubjects: "Clients (patients)",
            RetentionPeriod: "7 years after last treatment (or until client requests erasure)",
            SecurityMeasures: "AES-256-GCM at rest, TLS 1.2+ in transit, RBAC, audit logging"),

        new GdprRoPAEntryDto(
            ProcessingActivity: "Session Notes & Treatment Plans",
            Purpose: "Clinical documentation required for continuity of care",
            LegalBasis: "Art. 9(2)(h) GDPR — Healthcare provision",
            DataCategories: "Clinical observations, diagnoses, treatment goals (special category)",
            DataSubjects: "Clients (patients)",
            RetentionPeriod: "7 years after last treatment",
            SecurityMeasures: "AES-256-GCM encryption, signed/locked note workflow, therapist RBAC"),

        new GdprRoPAEntryDto(
            ProcessingActivity: "Appointment Scheduling",
            Purpose: "Managing therapist availability and client appointments",
            LegalBasis: "Art. 6(1)(b) GDPR — Performance of a contract",
            DataCategories: "Appointment times, therapist ID, client ID, session format",
            DataSubjects: "Clients and therapists",
            RetentionPeriod: "3 years after appointment date",
            SecurityMeasures: "Tenant isolation, RBAC, TLS in transit"),

        new GdprRoPAEntryDto(
            ProcessingActivity: "Consent Management",
            Purpose: "Recording and proving GDPR-compliant consent for data processing",
            LegalBasis: "Art. 7 GDPR — Conditions for consent",
            DataCategories: "Consent decision, timestamp, IP address (encrypted), template snapshot",
            DataSubjects: "Clients (data subjects)",
            RetentionPeriod: "Duration of processing + 3 years for proof of consent",
            SecurityMeasures: "Append-only audit log, INSERT-only DB role, AES-256-GCM encryption"),

        new GdprRoPAEntryDto(
            ProcessingActivity: "Document Storage",
            Purpose: "Storing intake forms, assessments, and other clinical documents",
            LegalBasis: "Art. 9(2)(h) GDPR — Healthcare provision",
            DataCategories: "Uploaded files (intake forms, assessments, consent forms)",
            DataSubjects: "Clients (patients)",
            RetentionPeriod: "7 years after last treatment",
            SecurityMeasures: "AES-256-GCM file encryption, per-tenant keys, streaming-only download"),

        new GdprRoPAEntryDto(
            ProcessingActivity: "Security Audit Logging",
            Purpose: "GDPR Art. 30 compliance; breach detection; access monitoring",
            LegalBasis: "Art. 6(1)(c) GDPR — Legal obligation; Art. 32 — Security of processing",
            DataCategories: "User ID, action type, resource accessed, IP address, timestamp",
            DataSubjects: "System users (therapists, admins)",
            RetentionPeriod: "5 years",
            SecurityMeasures: "Append-only log, INSERT-only DB role, admin-only query access"),
    ];
}
