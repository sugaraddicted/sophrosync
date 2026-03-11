using MediatR;
using Sophrosync.Notifications.Domain.Interfaces;

namespace Sophrosync.Notifications.Application.Commands.RetryFailed;

public sealed class RetryFailedNotificationCommandHandler(
    INotificationRepository repository) : IRequestHandler<RetryFailedNotificationCommand>
{
    public async Task Handle(RetryFailedNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await repository.GetByIdAsync(request.NotificationId, cancellationToken)
            ?? throw new InvalidOperationException($"Notification {request.NotificationId} not found.");
        notification.IncrementRetry();
        repository.Update(notification);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
