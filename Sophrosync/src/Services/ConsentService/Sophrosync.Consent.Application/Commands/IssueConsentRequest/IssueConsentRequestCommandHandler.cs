using MediatR;
using Sophrosync.Consent.Domain.Entities;
using Sophrosync.Consent.Domain.Interfaces;

namespace Sophrosync.Consent.Application.Commands.IssueConsentRequest;

public sealed class IssueConsentRequestCommandHandler(
    IConsentRequestRepository repository) : IRequestHandler<IssueConsentRequestCommand, Guid>
{
    public async Task<Guid> Handle(IssueConsentRequestCommand request, CancellationToken cancellationToken)
    {
        var consentRequest = ConsentRequest.Create(
            request.TenantId,
            request.ClientId,
            request.ConsentTemplateId,
            request.ExpiresAt);
        await repository.AddAsync(consentRequest, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return consentRequest.Id;
    }
}
