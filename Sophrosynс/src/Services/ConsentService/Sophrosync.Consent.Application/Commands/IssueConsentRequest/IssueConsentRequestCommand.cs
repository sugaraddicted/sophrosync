using MediatR;

namespace Sophrosync.Consent.Application.Commands.IssueConsentRequest;

public sealed record IssueConsentRequestCommand(
    Guid TenantId,
    Guid ClientId,
    Guid ConsentTemplateId,
    DateTime ExpiresAt) : IRequest<Guid>;
