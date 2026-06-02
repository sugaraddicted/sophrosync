using MediatR;

namespace Sophrosync.Reporting.Application.Commands.TriggerReportRun;

public sealed record TriggerReportRunCommand(
    Guid TenantId,
    Guid ReportDefinitionId,
    Guid RequestedByUserId,
    DateTime PeriodStart,
    DateTime PeriodEnd) : IRequest<Guid>;
