using MediatR;

namespace Sophrosync.Notifications.Application.Commands.DismissNotification;

public sealed record DismissNotificationCommand(Guid NotificationId) : IRequest;
