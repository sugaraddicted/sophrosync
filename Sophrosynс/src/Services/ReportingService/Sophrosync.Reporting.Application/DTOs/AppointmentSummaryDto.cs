namespace Sophrosync.Reporting.Application.DTOs;

public sealed record AppointmentSummaryDto(
    int TotalScheduled,
    int TotalCompleted,
    int TotalCancelled,
    int TotalNoShow,
    DateTime PeriodStart,
    DateTime PeriodEnd);
