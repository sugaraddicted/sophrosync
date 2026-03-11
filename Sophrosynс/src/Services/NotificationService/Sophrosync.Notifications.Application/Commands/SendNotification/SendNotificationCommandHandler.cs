using MediatR;
using Sophrosync.Notifications.Domain.Entities;
using Sophrosync.Notifications.Domain.Interfaces;

namespace Sophrosync.Notifications.Application.Commands.SendNotification;

public sealed class SendNotificationCommandHandler(
    INotificationRepository repository,
    IPublisher publisher) : IRequestHandler<SendNotificationCommand, Guid>
{
    public async Task<Guid> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByCorrelationIdAsync(request.CorrelationId, cancellationToken);
        if (existing is not null) return existing.Id;

        var notification = Notification.Create(
            request.TenantId,
            request.RecipientUserId,
            request.Channel,
            request.Type,
            request.Subject,
            request.Body,
            request.ScheduledFor,
            request.CorrelationId);

        await repository.AddAsync(notification, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        foreach (var evt in notification.DomainEvents)
            await publisher.Publish(evt, cancellationToken);

        notification.ClearDomainEvents();
        return notification.Id;
    }
}
