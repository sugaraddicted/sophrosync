using Microsoft.Extensions.Logging;
using Sophrosync.Notifications.Application.Interfaces;
using Sophrosync.Notifications.Domain.Entities;
using Sophrosync.Notifications.Domain.Enums;

namespace Sophrosync.Notifications.Infrastructure.Channels;

/// <summary>
/// In-app channel: marks notification as Sent so it appears in the user's inbox.
/// No external call required — the record itself IS the in-app notification.
/// </summary>
public sealed class InAppNotificationChannel(
    ILogger<InAppNotificationChannel> logger) : INotificationChannel
{
    public NotificationChannel Channel => NotificationChannel.InApp;

    public Task SendAsync(Notification notification, CancellationToken ct = default)
    {
        // In-app delivery is implicit — the record in DB is the notification.
        // This channel just logs and returns success.
        logger.LogInformation("In-app notification {Id} delivered for user {UserId}.",
            notification.Id, notification.RecipientUserId);
        return Task.CompletedTask;
    }
}
