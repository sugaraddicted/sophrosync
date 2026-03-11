using Sophrosync.Notifications.Domain.Enums;

namespace Sophrosync.Notifications.Application.DTOs;

public sealed record NotificationPreferenceDto(
    Guid Id,
    Guid TenantId,
    Guid? UserId,
    NotificationChannel PreferredChannel,
    bool EmailEnabled,
    bool InAppEnabled,
    bool SmsEnabled,
    string? EmailAddress);
