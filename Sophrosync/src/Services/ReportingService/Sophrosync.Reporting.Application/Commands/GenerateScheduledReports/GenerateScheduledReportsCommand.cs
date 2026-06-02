using MediatR;

namespace Sophrosync.Reporting.Application.Commands.GenerateScheduledReports;

public sealed record GenerateScheduledReportsCommand : IRequest<int>;
