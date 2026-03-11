using MediatR;
using Sophrosync.Consent.Domain.Interfaces;

namespace Sophrosync.Consent.Application.Commands.ExpireOverdueRequests;

public sealed class ExpireOverdueRequestsCommandHandler(
    IConsentRequestRepository repository,
    IPublisher publisher) : IRequestHandler<ExpireOverdueRequestsCommand, int>
{
    public async Task<int> Handle(ExpireOverdueRequestsCommand request, CancellationToken cancellationToken)
    {
        var overdue = await repository.GetOverdueAsync(DateTime.UtcNow, cancellationToken);
        foreach (var req in overdue)
        {
            req.Expire();
            repository.Update(req);
            foreach (var evt in req.DomainEvents) await publisher.Publish(evt, cancellationToken);
            req.ClearDomainEvents();
        }
        await repository.SaveChangesAsync(cancellationToken);
        return overdue.Count;
    }
}
