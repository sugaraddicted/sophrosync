using Sophrosync.Consent.Domain.Enums;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Consent.Domain.Events;

public sealed record ConsentTemplatePublishedDomainEvent(
    Guid TemplateId,
    Guid TenantId,
    ConsentPurpose Purpose,
    int VersionNumber) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
