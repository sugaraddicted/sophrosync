using MediatR;
using Sophrosync.Consent.Application.DTOs;
using Sophrosync.Consent.Domain.Interfaces;

namespace Sophrosync.Consent.Application.Queries.GetConsentTemplate;

public sealed class GetConsentTemplateQueryHandler(
    IConsentTemplateRepository repository) : IRequestHandler<GetConsentTemplateQuery, ConsentTemplateDto?>
{
    public async Task<ConsentTemplateDto?> Handle(GetConsentTemplateQuery request, CancellationToken cancellationToken)
    {
        var t = await repository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (t is null) return null;
        return new ConsentTemplateDto(t.Id, t.TenantId, t.Purpose, t.Title, t.BodyText,
            t.VersionNumber, t.Status, t.PublishedAt, t.CreatedAt);
    }
}
