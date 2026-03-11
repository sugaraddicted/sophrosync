using Sophrosync.Reporting.Domain.Enums;

namespace Sophrosync.Reporting.Application.DTOs;

public sealed record ReportRunDto(
    Guid Id,
    Guid TenantId,
    Guid ReportDefinitionId,
    Guid RequestedByUserId,
    ReportRunStatus Status,
    string? ResultJson,
    string? FailureReason,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    DateTime? CompletedAt,
    DateTime CreatedAt);
