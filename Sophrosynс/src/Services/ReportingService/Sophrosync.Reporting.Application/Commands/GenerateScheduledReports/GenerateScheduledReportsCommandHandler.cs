using MediatR;
using Sophrosync.Reporting.Application.Commands.TriggerReportRun;
using Sophrosync.Reporting.Domain.Interfaces;

namespace Sophrosync.Reporting.Application.Commands.GenerateScheduledReports;

public sealed class GenerateScheduledReportsCommandHandler(
    IReportDefinitionRepository repository,
    IMediator mediator) : IRequestHandler<GenerateScheduledReportsCommand, int>
{
    public async Task<int> Handle(GenerateScheduledReportsCommand request, CancellationToken cancellationToken)
    {
        var due = await repository.GetScheduledDueAsync(DateTime.UtcNow, cancellationToken);
        var count = 0;

        foreach (var definition in due)
        {
            await mediator.Send(new TriggerReportRunCommand(
                definition.TenantId,
                definition.Id,
                Guid.Empty, // system-triggered
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow), cancellationToken);
            definition.RecordRun();
            repository.Update(definition);
            count++;
        }

        await repository.SaveChangesAsync(cancellationToken);
        return count;
    }
}
