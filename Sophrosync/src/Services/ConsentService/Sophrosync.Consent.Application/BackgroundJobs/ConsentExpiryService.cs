using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sophrosync.Consent.Application.Commands.ExpireOverdueRequests;

namespace Sophrosync.Consent.Application.BackgroundJobs;

/// <summary>
/// Background service that runs daily to expire stale consent requests.
/// </summary>
public sealed class ConsentExpiryService(
    IServiceScopeFactory scopeFactory,
    ILogger<ConsentExpiryService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ConsentExpiryService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var expired = await mediator.Send(new ExpireOverdueRequestsCommand(), stoppingToken);
                logger.LogInformation("Expired {Count} overdue consent requests.", expired);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during consent expiry processing.");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }
}
