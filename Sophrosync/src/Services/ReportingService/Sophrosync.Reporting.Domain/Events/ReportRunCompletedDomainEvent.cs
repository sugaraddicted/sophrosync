using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Reporting.Domain.Events;

public sealed record ReportRunCompletedDomainEvent(
    Guid RunId,
    Guid TenantId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
