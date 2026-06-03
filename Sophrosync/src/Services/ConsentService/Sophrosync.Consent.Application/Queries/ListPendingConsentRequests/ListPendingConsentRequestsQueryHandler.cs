using MediatR;
using Sophrosync.Consent.Application.DTOs;
using Sophrosync.Consent.Domain.Interfaces;

namespace Sophrosync.Consent.Application.Queries.ListPendingConsentRequests;

public sealed class ListPendingConsentRequestsQueryHandler(
    IConsentRequestRepository repository) : IRequestHandler<ListPendingConsentRequestsQuery, IReadOnlyList<ConsentRequestDto>>
{
    public async Task<IReadOnlyList<ConsentRequestDto>> Handle(ListPendingConsentRequestsQuery request, CancellationToken cancellationToken)
    {
        var requests = await repository.GetPendingForClientAsync(request.ClientId, cancellationToken);
        return requests.Select(r => new ConsentRequestDto(
            r.Id, r.TenantId, r.ClientId, r.ConsentTemplateId,
            r.Status, r.ExpiresAt, r.CompletedAt, r.CreatedAt)).ToList();
    }
}
