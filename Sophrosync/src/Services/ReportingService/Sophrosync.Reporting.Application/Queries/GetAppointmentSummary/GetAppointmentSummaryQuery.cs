using MediatR;
using Sophrosync.Reporting.Application.DTOs;

namespace Sophrosync.Reporting.Application.Queries.GetAppointmentSummary;

public sealed record GetAppointmentSummaryQuery(
    Guid TenantId,
    DateTime PeriodStart,
    DateTime PeriodEnd) : IRequest<AppointmentSummaryDto>;
