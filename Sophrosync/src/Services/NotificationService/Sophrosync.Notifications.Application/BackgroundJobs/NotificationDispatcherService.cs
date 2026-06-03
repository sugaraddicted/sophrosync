using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sophrosync.Notifications.Application.Interfaces;
using Sophrosync.Notifications.Domain.Enums;
using Sophrosync.Notifications.Domain.Interfaces;

namespace Sophrosync.Notifications.Application.BackgroundJobs;

/// <summary>
/// Background service that polls the notifications table every 60 seconds
/// for pending notifications where ScheduledFor &lt;= NOW() and dispatches them
/// via the appropriate INotificationChannel.
/// </summary>
public sealed class NotificationDispatcherService(
    IServiceScopeFactory scopeFactory,
    ILogger<NotificationDispatcherService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(60);
    private const int MaxRetries = 3;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("NotificationDispatcherService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingNotificationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing pending notifications.");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task ProcessPendingNotificationsAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        var channels = scope.ServiceProvider.GetServices<INotificationChannel>()
            .ToDictionary(c => c.Channel);

        var pending = await repository.GetPendingDueAsync(DateTime.UtcNow, ct);
        logger.LogInformation("Processing {Count} pending notifications.", pending.Count);

        foreach (var notification in pending)
        {
            if (!channels.TryGetValue(notification.Channel, out var channel))
            {
                logger.LogWarning("No channel handler for {Channel}", notification.Channel);
                continue;
            }

            var attempt = 0;
            while (attempt < MaxRetries)
            {
                try
                {
                    await channel.SendAsync(notification, ct);
                    notification.MarkSent();
                    repository.Update(notification);
                    await repository.SaveChangesAsync(ct);
                    break;
                }
                catch (Exception ex)
                {
                    attempt++;
                    logger.LogWarning(ex, "Send attempt {Attempt} failed for notification {Id}.", attempt, notification.Id);
                    if (attempt >= MaxRetries)
                    {
                        notification.MarkFailed(ex.Message);
                        repository.Update(notification);
                        await repository.SaveChangesAsync(ct);
                    }
                    else
                    {
                        notification.IncrementRetry();
                        repository.Update(notification);
                        await repository.SaveChangesAsync(ct);
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), ct);
                    }
                }
            }
        }
    }
}
