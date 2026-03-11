using MediatR;
using Sophrosync.Consent.Domain.Interfaces;

namespace Sophrosync.Consent.Application.Commands.RevokeConsentRequest;

public sealed class RevokeConsentRequestCommandHandler(
    IConsentRequestRepository repository) : IRequestHandler<RevokeConsentRequestCommand>
{
    public async Task Handle(RevokeConsentRequestCommand request, CancellationToken cancellationToken)
    {
        var consentRequest = await repository.GetByIdAsync(request.ConsentRequestId, cancellationToken)
            ?? throw new InvalidOperationException($"ConsentRequest {request.ConsentRequestId} not found.");
        consentRequest.Revoke();
        repository.Update(consentRequest);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
