using MediatR;
using Sophrosync.Reporting.Domain.Entities;
using Sophrosync.Reporting.Domain.Interfaces;

namespace Sophrosync.Reporting.Application.Commands.TriggerReportRun;

public sealed class TriggerReportRunCommandHandler(
    IReportDefinitionRepository definitionRepository,
    IReportRunRepository runRepository,
    IPublisher publisher) : IRequestHandler<TriggerReportRunCommand, Guid>
{
    public async Task<Guid> Handle(TriggerReportRunCommand request, CancellationToken cancellationToken)
    {
        var definition = await definitionRepository.GetByIdAsync(request.ReportDefinitionId, cancellationToken)
            ?? throw new InvalidOperationException($"ReportDefinition {request.ReportDefinitionId} not found.");

        var run = ReportRun.Create(
            request.TenantId,
            request.ReportDefinitionId,
            request.RequestedByUserId,
            request.PeriodStart,
            request.PeriodEnd);

        await runRepository.AddAsync(run, cancellationToken);
        await runRepository.SaveChangesAsync(cancellationToken);

        foreach (var evt in run.DomainEvents) await publisher.Publish(evt, cancellationToken);
        run.ClearDomainEvents();

        return run.Id;
    }
}
