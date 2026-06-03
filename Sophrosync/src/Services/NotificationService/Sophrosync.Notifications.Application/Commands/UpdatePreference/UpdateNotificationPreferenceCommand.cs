using MediatR;
using Sophrosync.Notifications.Domain.Enums;

namespace Sophrosync.Notifications.Application.Commands.UpdatePreference;

public sealed record UpdateNotificationPreferenceCommand(
    Guid TenantId,
    Guid UserId,
    NotificationChannel PreferredChannel,
    bool EmailEnabled,
    bool InAppEnabled,
    bool SmsEnabled,
    string? EmailAddress) : IRequest;
