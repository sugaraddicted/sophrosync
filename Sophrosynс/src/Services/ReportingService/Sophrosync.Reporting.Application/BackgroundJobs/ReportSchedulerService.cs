using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sophrosync.Reporting.Application.Commands.GenerateScheduledReports;

namespace Sophrosync.Reporting.Application.BackgroundJobs;

/// <summary>
/// Background service that polls daily for report definitions with due schedules
/// and triggers report runs for each.
/// </summary>
public sealed class ReportSchedulerService(
    IServiceScopeFactory scopeFactory,
    ILogger<ReportSchedulerService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ReportSchedulerService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var count = await mediator.Send(new GenerateScheduledReportsCommand(), stoppingToken);
                logger.LogInformation("Triggered {Count} scheduled reports.", count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during scheduled report generation.");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }
}
