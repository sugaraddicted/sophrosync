using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Sophrosync.Reporting.Application.DTOs;
using Sophrosync.Reporting.Application.Interfaces;

namespace Sophrosync.Reporting.Application.Queries.GetClinicalOutcomeSummary;

public sealed class GetClinicalOutcomeSummaryQueryHandler(
    IClientServiceClient clientServiceClient,
    IMemoryCache cache) : IRequestHandler<GetClinicalOutcomeSummaryQuery, ClinicalOutcomeSummaryDto>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public async Task<ClinicalOutcomeSummaryDto> Handle(GetClinicalOutcomeSummaryQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"clinical_outcomes_{request.TenantId}_{request.PeriodStart:yyyyMMdd}_{request.PeriodEnd:yyyyMMdd}";

        if (cache.TryGetValue(cacheKey, out ClinicalOutcomeSummaryDto? cached) && cached is not null)
            return cached;

        var summary = await clientServiceClient.GetClientSummaryAsync(
            request.TenantId, request.PeriodStart, request.PeriodEnd, cancellationToken);

        cache.Set(cacheKey, summary, CacheTtl);
        return summary;
    }
}
