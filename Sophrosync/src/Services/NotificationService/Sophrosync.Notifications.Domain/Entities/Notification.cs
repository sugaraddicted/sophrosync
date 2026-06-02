using Sophrosync.Notifications.Domain.Enums;
using Sophrosync.Notifications.Domain.Events;
using Sophrosync.SharedKernel.Domain;

namespace Sophrosync.Notifications.Domain.Entities;

public sealed class Notification : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid RecipientUserId { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public NotificationType Type { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty; // AES-256-GCM encrypted at DB level
    public NotificationStatus Status { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime ScheduledFor { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime? DismissedAt { get; private set; }
    public string CorrelationId { get; private set; } = string.Empty;
    public string? FailureReason { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private Notification() { }

    public static Notification Create(
        Guid tenantId,
        Guid recipientUserId,
        NotificationChannel channel,
        NotificationType type,
        string subject,
        string body,
        DateTime scheduledFor,
        string correlationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);

        var notification = new Notification
        {
            TenantId = tenantId,
            RecipientUserId = recipientUserId,
            Channel = channel,
            Type = type,
            Subject = subject,
            Body = body,
            Status = NotificationStatus.Pending,
            RetryCount = 0,
            ScheduledFor = scheduledFor,
            CorrelationId = correlationId
        };

        notification.RaiseDomainEvent(new NotificationCreatedDomainEvent(notification.Id, tenantId, recipientUserId, type));
        return notification;
    }

    public void MarkSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
        RaiseDomainEvent(new NotificationSentDomainEvent(Id, TenantId, RecipientUserId, Channel));
    }

    public void MarkFailed(string reason)
    {
        Status = NotificationStatus.Failed;
        FailureReason = reason;
        RaiseDomainEvent(new NotificationFailedDomainEvent(Id, TenantId, FailureReason));
    }

    public void IncrementRetry()
    {
        RetryCount++;
        Status = NotificationStatus.Retrying;
    }

    public void Dismiss()
    {
        Status = NotificationStatus.Dismissed;
        DismissedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
    }
}
