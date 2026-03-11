using Sophrosync.Notifications.Domain.Enums;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Notifications.Domain.Events;

public sealed record NotificationCreatedDomainEvent(
    Guid NotificationId,
    Guid TenantId,
    Guid RecipientUserId,
    NotificationType Type) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
