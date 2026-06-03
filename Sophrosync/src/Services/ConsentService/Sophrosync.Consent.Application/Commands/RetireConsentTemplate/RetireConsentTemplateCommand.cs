using MediatR;

namespace Sophrosync.Consent.Application.Commands.RetireConsentTemplate;

public sealed record RetireConsentTemplateCommand(Guid TemplateId) : IRequest;
