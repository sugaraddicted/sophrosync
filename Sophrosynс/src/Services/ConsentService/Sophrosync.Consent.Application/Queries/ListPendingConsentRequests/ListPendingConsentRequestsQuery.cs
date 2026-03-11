using MediatR;
using Sophrosync.Consent.Application.DTOs;

namespace Sophrosync.Consent.Application.Queries.ListPendingConsentRequests;

public sealed record ListPendingConsentRequestsQuery(Guid ClientId) : IRequest<IReadOnlyList<ConsentRequestDto>>;
