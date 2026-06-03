using MediatR;
using Sophrosync.Reporting.Application.DTOs;

namespace Sophrosync.Reporting.Application.Queries.GetReportRun;

public sealed record GetReportRunQuery(Guid RunId) : IRequest<ReportRunDto?>;
