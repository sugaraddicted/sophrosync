using MediatR;

namespace Sophrosync.Consent.Application.Commands.RevokeConsentRequest;

public sealed record RevokeConsentRequestCommand(Guid ConsentRequestId) : IRequest;
