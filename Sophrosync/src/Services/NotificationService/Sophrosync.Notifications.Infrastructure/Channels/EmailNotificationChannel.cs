using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sophrosync.Notifications.Application.Interfaces;
using Sophrosync.Notifications.Domain.Entities;
using Sophrosync.Notifications.Domain.Enums;
using Sophrosync.Notifications.Domain.Interfaces;

namespace Sophrosync.Notifications.Infrastructure.Channels;

public sealed class EmailNotificationChannel(
    IConfiguration configuration,
    INotificationPreferenceRepository preferenceRepository,
    ILogger<EmailNotificationChannel> logger) : INotificationChannel
{
    public NotificationChannel Channel => NotificationChannel.Email;

    public async Task SendAsync(Notification notification, CancellationToken ct = default)
    {
        var prefs = await preferenceRepository.GetForUserAsync(notification.TenantId, notification.RecipientUserId, ct);
        var recipientEmail = prefs?.EmailAddress;

        if (string.IsNullOrWhiteSpace(recipientEmail))
        {
            logger.LogWarning(
                "No email address configured for user {UserId}, skipping email notification {Id}.",
                notification.RecipientUserId, notification.Id);
            return;
        }

        var smtpSection = configuration.GetSection("Smtp");
        var host = smtpSection["Host"] ?? "localhost";
        var port = int.Parse(smtpSection["Port"] ?? "1025");
        var from = smtpSection["From"] ?? "noreply@sophrosync.com";

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(from));
        message.To.Add(MailboxAddress.Parse(recipientEmail));
        message.Subject = notification.Subject;
        message.Body = new TextPart("plain") { Text = notification.Body };

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, false, ct);

        var username = smtpSection["Username"];
        var password = smtpSection["Password"] ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(username))
            await client.AuthenticateAsync(username, password, ct);

        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);

        logger.LogInformation("Email sent to {Email} for notification {Id}.", recipientEmail, notification.Id);
    }
}
