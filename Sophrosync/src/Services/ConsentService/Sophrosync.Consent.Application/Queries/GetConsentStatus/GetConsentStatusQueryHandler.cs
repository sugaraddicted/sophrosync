using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Sophrosync.Consent.Application.DTOs;
using Sophrosync.Consent.Domain.Enums;
using Sophrosync.Consent.Domain.Interfaces;

namespace Sophrosync.Consent.Application.Queries.GetConsentStatus;

public sealed class GetConsentStatusQueryHandler(
    IConsentRecordRepository recordRepository,
    IMemoryCache cache) : IRequestHandler<GetConsentStatusQuery, ConsentStatusDto>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    public async Task<ConsentStatusDto> Handle(GetConsentStatusQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"consent_status_{request.TenantId}_{request.ClientId}_{request.Purpose}";

        if (cache.TryGetValue(cacheKey, out ConsentStatusDto? cached) && cached is not null)
            return cached;

        var latest = await recordRepository.GetLatestForClientAndPurposeAsync(
            request.ClientId, request.Purpose, cancellationToken);

        var hasActiveConsent = latest?.Action == ConsentAction.Granted;
        var result = new ConsentStatusDto(
            request.ClientId,
            request.Purpose,
            hasActiveConsent,
            hasActiveConsent ? latest!.CreatedAt : null,
            null);

        cache.Set(cacheKey, result, CacheTtl);
        return result;
    }
}
