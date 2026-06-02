using MediatR;

namespace Sophrosync.Notifications.Application.Commands.RetryFailed;

public sealed record RetryFailedNotificationCommand(Guid NotificationId) : IRequest;
