using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Schedule.Domain.Events;

public sealed record AppointmentScheduledDomainEvent(
    Guid AppointmentId,
    Guid TenantId,
    Guid ClientId,
    Guid TherapistId,
    DateTime ScheduledAt) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
