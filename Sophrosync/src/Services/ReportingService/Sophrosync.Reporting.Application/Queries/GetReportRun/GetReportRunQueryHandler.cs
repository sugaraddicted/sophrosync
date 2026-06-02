using MediatR;
using Sophrosync.Reporting.Application.DTOs;
using Sophrosync.Reporting.Domain.Interfaces;

namespace Sophrosync.Reporting.Application.Queries.GetReportRun;

public sealed class GetReportRunQueryHandler(
    IReportRunRepository repository) : IRequestHandler<GetReportRunQuery, ReportRunDto?>
{
    public async Task<ReportRunDto?> Handle(GetReportRunQuery request, CancellationToken cancellationToken)
    {
        var run = await repository.GetByIdAsync(request.RunId, cancellationToken);
        if (run is null) return null;
        return new ReportRunDto(run.Id, run.TenantId, run.ReportDefinitionId, run.RequestedByUserId,
            run.Status, run.ResultJson, run.FailureReason, run.PeriodStart, run.PeriodEnd,
            run.CompletedAt, run.CreatedAt);
    }
}
