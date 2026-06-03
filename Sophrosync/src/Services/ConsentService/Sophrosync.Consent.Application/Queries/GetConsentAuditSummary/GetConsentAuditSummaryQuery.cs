using MediatR;
using Sophrosync.Consent.Application.DTOs;

namespace Sophrosync.Consent.Application.Queries.GetConsentAuditSummary;

public sealed record GetConsentAuditSummaryQuery : IRequest<IReadOnlyList<ConsentRecordDto>>;
