using Sophrosync.Consent.Domain.Enums;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Consent.Domain.Events;

public sealed record ConsentRecordedDomainEvent(
    Guid RecordId,
    Guid TenantId,
    Guid ClientId,
    ConsentPurpose Purpose,
    ConsentAction Action) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
