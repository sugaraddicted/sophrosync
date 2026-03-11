using MediatR;

namespace Sophrosync.Consent.Application.Commands.PublishConsentTemplate;

public sealed record PublishConsentTemplateCommand(Guid TemplateId) : IRequest;
