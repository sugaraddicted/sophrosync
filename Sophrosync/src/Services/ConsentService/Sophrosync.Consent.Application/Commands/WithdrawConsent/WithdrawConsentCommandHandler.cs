using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Sophrosync.Consent.Domain.Entities;
using Sophrosync.Consent.Domain.Enums;
using Sophrosync.Consent.Domain.Interfaces;

namespace Sophrosync.Consent.Application.Commands.WithdrawConsent;

public sealed class WithdrawConsentCommandHandler(
    IConsentRequestRepository requestRepository,
    IConsentTemplateRepository templateRepository,
    IConsentRecordRepository recordRepository,
    IPublisher publisher,
    IMemoryCache cache) : IRequestHandler<WithdrawConsentCommand, Guid>
{
    public async Task<Guid> Handle(WithdrawConsentCommand request, CancellationToken cancellationToken)
    {
        var consentRequest = await requestRepository.GetByIdAsync(request.ConsentRequestId, cancellationToken)
            ?? throw new InvalidOperationException($"ConsentRequest {request.ConsentRequestId} not found.");

        var template = await templateRepository.GetByIdAsync(consentRequest.ConsentTemplateId, cancellationToken)
            ?? throw new InvalidOperationException("Associated ConsentTemplate not found.");

        var record = ConsentRecord.Create(
            consentRequest.TenantId,
            consentRequest.ClientId,
            consentRequest.Id,
            template.Id,
            template.Purpose,
            ConsentAction.Withdrawn,
            request.IpAddress,
            template.BodyText,
            template.VersionNumber);

        await recordRepository.AddAsync(record, cancellationToken);
        await recordRepository.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        var cacheKey = $"consent_status_{consentRequest.TenantId}_{consentRequest.ClientId}_{template.Purpose}";
        cache.Remove(cacheKey);

        foreach (var evt in record.DomainEvents) await publisher.Publish(evt, cancellationToken);
        record.ClearDomainEvents();

        return record.Id;
    }
}
