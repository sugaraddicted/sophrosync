using Sophrosync.Notifications.Domain.Enums;

namespace Sophrosync.Notifications.Application.DTOs;

public sealed record NotificationDto(
    Guid Id,
    Guid TenantId,
    Guid RecipientUserId,
    NotificationChannel Channel,
    NotificationType Type,
    string Subject,
    string Body,
    NotificationStatus Status,
    int RetryCount,
    DateTime ScheduledFor,
    DateTime? SentAt,
    DateTime? DismissedAt,
    string CorrelationId,
    string? FailureReason,
    DateTime CreatedAt);
