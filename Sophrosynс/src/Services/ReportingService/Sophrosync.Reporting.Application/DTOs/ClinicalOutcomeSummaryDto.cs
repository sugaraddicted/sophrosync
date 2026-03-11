namespace Sophrosync.Reporting.Application.DTOs;

public sealed record ClinicalOutcomeSummaryDto(
    Guid TenantId,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    int TotalClientsActive,
    int TotalSessionsCompleted,
    int TotalTreatmentPlansActive,
    int GoalsAchieved,
    int GoalsInProgress,
    double AverageSessionsPerClient);
