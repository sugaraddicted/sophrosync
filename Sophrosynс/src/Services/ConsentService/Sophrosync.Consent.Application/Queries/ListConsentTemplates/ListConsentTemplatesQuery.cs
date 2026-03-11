using MediatR;
using Sophrosync.Consent.Application.DTOs;

namespace Sophrosync.Consent.Application.Queries.ListConsentTemplates;

public sealed record ListConsentTemplatesQuery : IRequest<IReadOnlyList<ConsentTemplateDto>>;
