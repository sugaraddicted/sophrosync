using MediatR;
using Sophrosync.Reporting.Domain.Enums;
using Sophrosync.Reporting.Domain.ValueObjects;

namespace Sophrosync.Reporting.Application.Commands.CreateReportDefinition;

public sealed record CreateReportDefinitionCommand(
    Guid TenantId,
    string Name,
    ReportType Type,
    ReportFormat Format,
    ReportSchedule? Schedule = null) : IRequest<Guid>;
