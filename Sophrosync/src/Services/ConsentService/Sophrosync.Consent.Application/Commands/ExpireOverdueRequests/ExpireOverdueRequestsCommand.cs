using MediatR;

namespace Sophrosync.Consent.Application.Commands.ExpireOverdueRequests;

public sealed record ExpireOverdueRequestsCommand : IRequest<int>;
