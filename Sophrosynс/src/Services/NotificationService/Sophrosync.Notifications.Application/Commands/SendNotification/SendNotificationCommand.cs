using MediatR;
using Sophrosync.Notifications.Domain.Enums;

namespace Sophrosync.Notifications.Application.Commands.SendNotification;

public sealed record SendNotificationCommand(
    Guid TenantId,
    Guid RecipientUserId,
    NotificationChannel Channel,
    NotificationType Type,
    string Subject,
    string Body,
    DateTime ScheduledFor,
    string CorrelationId) : IRequest<Guid>;
