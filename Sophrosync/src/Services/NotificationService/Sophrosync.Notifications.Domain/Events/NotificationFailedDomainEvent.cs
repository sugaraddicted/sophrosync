using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Notifications.Domain.Events;

public sealed record NotificationFailedDomainEvent(
    Guid NotificationId,
    Guid TenantId,
    string Reason) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
