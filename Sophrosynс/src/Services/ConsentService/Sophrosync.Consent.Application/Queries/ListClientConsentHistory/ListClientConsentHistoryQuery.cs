using MediatR;
using Sophrosync.Consent.Application.DTOs;

namespace Sophrosync.Consent.Application.Queries.ListClientConsentHistory;

public sealed record ListClientConsentHistoryQuery(Guid ClientId) : IRequest<IReadOnlyList<ConsentRecordDto>>;
