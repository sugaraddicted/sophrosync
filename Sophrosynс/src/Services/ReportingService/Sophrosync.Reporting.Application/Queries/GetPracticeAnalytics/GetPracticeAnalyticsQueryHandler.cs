using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Sophrosync.Reporting.Application.DTOs;
using Sophrosync.Reporting.Application.Interfaces;

namespace Sophrosync.Reporting.Application.Queries.GetPracticeAnalytics;

public sealed class GetPracticeAnalyticsQueryHandler(
    IScheduleServiceClient scheduleServiceClient,
    IMemoryCache cache) : IRequestHandler<GetPracticeAnalyticsQuery, PracticeAnalyticsSummaryDto>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public async Task<PracticeAnalyticsSummaryDto> Handle(GetPracticeAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"practice_analytics_{request.TenantId}_{request.PeriodStart:yyyyMMdd}_{request.PeriodEnd:yyyyMMdd}";

        if (cache.TryGetValue(cacheKey, out PracticeAnalyticsSummaryDto? cached) && cached is not null)
            return cached;

        var apptSummary = await scheduleServiceClient.GetAppointmentSummaryAsync(
            request.TenantId, request.PeriodStart, request.PeriodEnd, cancellationToken);

        var result = new PracticeAnalyticsSummaryDto(
            request.TenantId,
            request.PeriodStart,
            request.PeriodEnd,
            apptSummary.TotalScheduled,
            apptSummary.TotalCompleted,
            apptSummary.TotalCancelled,
            apptSummary.TotalNoShow,
            apptSummary.TotalScheduled > 0 ? (double)apptSummary.TotalCancelled / apptSummary.TotalScheduled : 0,
            apptSummary.TotalScheduled > 0 ? (double)apptSummary.TotalNoShow / apptSummary.TotalScheduled : 0,
            0, // NewClientsOnboarded - would come from ClientService
            0  // ActiveTherapists - would come from ClientService
        );

        cache.Set(cacheKey, result, CacheTtl);
        return result;
    }
}
