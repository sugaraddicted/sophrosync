using MediatR;
using Sophrosync.Consent.Application.DTOs;

namespace Sophrosync.Consent.Application.Queries.GetConsentTemplate;

public sealed record GetConsentTemplateQuery(Guid TemplateId) : IRequest<ConsentTemplateDto?>;
