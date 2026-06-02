using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Reporting.Domain.Events;

public sealed record ReportRunCreatedDomainEvent(
    Guid RunId,
    Guid TenantId,
    Guid ReportDefinitionId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
