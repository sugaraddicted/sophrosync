using MediatR;

namespace Sophrosync.SharedKernel.Abstractions;

public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
