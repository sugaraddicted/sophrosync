using MediatR;

namespace Sophrosync.Consent.Application.Commands.WithdrawConsent;

public sealed record WithdrawConsentCommand(
    Guid ClientId,
    Guid ConsentRequestId,
    string IpAddress) : IRequest<Guid>;
