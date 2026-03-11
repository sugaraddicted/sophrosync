using MediatR;
using Sophrosync.Consent.Domain.Enums;

namespace Sophrosync.Consent.Application.Commands.CreateConsentTemplate;

public sealed record CreateConsentTemplateCommand(
    Guid TenantId,
    ConsentPurpose Purpose,
    string Title,
    string BodyText) : IRequest<Guid>;
