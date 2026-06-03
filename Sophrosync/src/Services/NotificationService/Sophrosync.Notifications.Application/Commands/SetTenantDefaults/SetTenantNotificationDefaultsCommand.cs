using MediatR;
using Sophrosync.Notifications.Domain.Enums;

namespace Sophrosync.Notifications.Application.Commands.SetTenantDefaults;

public sealed record SetTenantNotificationDefaultsCommand(
    Guid TenantId,
    NotificationChannel PreferredChannel,
    bool EmailEnabled,
    bool InAppEnabled,
    bool SmsEnabled) : IRequest;
