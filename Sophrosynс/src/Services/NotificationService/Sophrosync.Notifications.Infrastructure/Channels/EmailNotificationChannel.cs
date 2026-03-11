using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sophrosync.Notifications.Application.Interfaces;
using Sophrosync.Notifications.Domain.Entities;
using Sophrosync.Notifications.Domain.Enums;

namespace Sophrosync.Notifications.Infrastructure.Channels;

public sealed class EmailNotificationChannel(
    IConfiguration configuration,
    ILogger<EmailNotificationChannel> logger) : INotificationChannel
{
    public NotificationChannel Channel => NotificationChannel.Email;

    public async Task SendAsync(Notification notification, CancellationToken ct = default)
    {
        var smtpConfig = configuration.GetSection("Smtp");
        var host = smtpConfig["Host"] ?? "localhost";
        var port = int.Parse(smtpConfig["Port"] ?? "587");
        var from = smtpConfig["From"] ?? "noreply@sophrosync.com";

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(from));
        // In real impl, look up recipient email from preferences/identity service
        message.To.Add(MailboxAddress.Parse($"user-{notification.RecipientUserId}@internal.sophrosync.com"));
        message.Subject = notification.Subject;
        message.Body = new TextPart("plain") { Text = notification.Body };

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, false, ct);
        if (!string.IsNullOrEmpty(smtpConfig["Username"]))
            await client.AuthenticateAsync(smtpConfig["Username"], smtpConfig["Password"], ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);

        logger.LogInformation("Email sent for notification {Id}", notification.Id);
    }
}
