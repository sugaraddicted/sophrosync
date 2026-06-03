using MediatR;
using Sophrosync.Reporting.Domain.Interfaces;

namespace Sophrosync.Reporting.Application.Commands.DeleteReportRun;

public sealed class DeleteReportRunCommandHandler(
    IReportRunRepository repository) : IRequestHandler<DeleteReportRunCommand>
{
    public async Task Handle(DeleteReportRunCommand request, CancellationToken cancellationToken)
    {
        var run = await repository.GetByIdAsync(request.RunId, cancellationToken)
            ?? throw new InvalidOperationException($"ReportRun {request.RunId} not found.");
        run.DeleteResult();
        repository.Update(run);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
