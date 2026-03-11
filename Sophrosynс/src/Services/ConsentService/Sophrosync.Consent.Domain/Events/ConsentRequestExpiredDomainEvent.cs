using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Consent.Domain.Events;

public sealed record ConsentRequestExpiredDomainEvent(
    Guid RequestId,
    Guid TenantId,
    Guid ClientId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
