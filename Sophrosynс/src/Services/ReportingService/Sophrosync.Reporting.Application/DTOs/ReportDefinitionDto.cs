using Sophrosync.Reporting.Domain.Enums;

namespace Sophrosync.Reporting.Application.DTOs;

public sealed record ReportDefinitionDto(
    Guid Id,
    Guid TenantId,
    string Name,
    ReportType Type,
    ReportFormat Format,
    bool IsActive,
    DateTime? LastRunAt,
    DateTime CreatedAt);
