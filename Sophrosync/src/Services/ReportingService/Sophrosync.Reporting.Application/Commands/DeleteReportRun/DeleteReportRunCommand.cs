using MediatR;

namespace Sophrosync.Reporting.Application.Commands.DeleteReportRun;

public sealed record DeleteReportRunCommand(Guid RunId) : IRequest;
