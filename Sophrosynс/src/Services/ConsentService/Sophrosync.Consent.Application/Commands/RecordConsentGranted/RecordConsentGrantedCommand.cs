using MediatR;

namespace Sophrosync.Consent.Application.Commands.RecordConsentGranted;

public sealed record RecordConsentGrantedCommand(
    Guid ConsentRequestId,
    string IpAddress) : IRequest<Guid>;
