using MediatR;
using Sophrosync.Notifications.Domain.Interfaces;

namespace Sophrosync.Notifications.Application.Commands.DismissNotification;

public sealed class DismissNotificationCommandHandler(
    INotificationRepository repository) : IRequestHandler<DismissNotificationCommand>
{
    public async Task Handle(DismissNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await repository.GetByIdAsync(request.NotificationId, cancellationToken)
            ?? throw new InvalidOperationException($"Notification {request.NotificationId} not found.");
        notification.Dismiss();
        repository.Update(notification);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
