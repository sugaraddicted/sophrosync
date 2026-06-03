using MediatR;
using Sophrosync.Reporting.Application.DTOs;

namespace Sophrosync.Reporting.Application.Queries.GetPracticeAnalytics;

public sealed record GetPracticeAnalyticsQuery(
    Guid TenantId,
    DateTime PeriodStart,
    DateTime PeriodEnd) : IRequest<PracticeAnalyticsSummaryDto>;
