using Sophrosync.Notifications.Domain.Enums;
using Sophrosync.SharedKernel.Domain;

namespace Sophrosync.Notifications.Domain.Entities;

public sealed class NotificationPreference : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid? UserId { get; private set; } // null = tenant-level default
    public NotificationChannel PreferredChannel { get; private set; }
    public bool EmailEnabled { get; private set; }
    public bool InAppEnabled { get; private set; }
    public bool SmsEnabled { get; private set; }
    public string? EmailAddress { get; private set; }

    private NotificationPreference() { }

    public static NotificationPreference Create(
        Guid tenantId,
        Guid? userId,
        NotificationChannel preferredChannel,
        bool emailEnabled,
        bool inAppEnabled,
        bool smsEnabled,
        string? emailAddress = null)
    {
        return new NotificationPreference
        {
            TenantId = tenantId,
            UserId = userId,
            PreferredChannel = preferredChannel,
            EmailEnabled = emailEnabled,
            InAppEnabled = inAppEnabled,
            SmsEnabled = smsEnabled,
            EmailAddress = emailAddress
        };
    }

    public void Update(
        NotificationChannel preferredChannel,
        bool emailEnabled,
        bool inAppEnabled,
        bool smsEnabled,
        string? emailAddress)
    {
        PreferredChannel = preferredChannel;
        EmailEnabled = emailEnabled;
        InAppEnabled = inAppEnabled;
        SmsEnabled = smsEnabled;
        EmailAddress = emailAddress;
    }
}
