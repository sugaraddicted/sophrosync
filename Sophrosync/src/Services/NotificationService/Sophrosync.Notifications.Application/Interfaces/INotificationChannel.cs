using Sophrosync.Notifications.Domain.Entities;
using Sophrosync.Notifications.Domain.Enums;

namespace Sophrosync.Notifications.Application.Interfaces;

public interface INotificationChannel
{
    NotificationChannel Channel { get; }
    Task SendAsync(Notification notification, CancellationToken ct = default);
}
