namespace Sophrosync.Reporting.Application.DTOs;

public sealed record PracticeAnalyticsSummaryDto(
    Guid TenantId,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    int TotalAppointments,
    int CompletedAppointments,
    int CancelledAppointments,
    int NoShowAppointments,
    double CancellationRate,
    double NoShowRate,
    int NewClientsOnboarded,
    int ActiveTherapists);
