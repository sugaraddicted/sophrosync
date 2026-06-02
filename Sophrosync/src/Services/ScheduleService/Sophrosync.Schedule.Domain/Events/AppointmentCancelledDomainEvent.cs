using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Schedule.Domain.Events;

public sealed record AppointmentCancelledDomainEvent(
    Guid AppointmentId,
    Guid TenantId,
    string Reason) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
