using MediatR;
using Sophrosync.Consent.Application.DTOs;
using Sophrosync.Consent.Domain.Interfaces;

namespace Sophrosync.Consent.Application.Queries.ListConsentTemplates;

public sealed class ListConsentTemplatesQueryHandler(
    IConsentTemplateRepository repository) : IRequestHandler<ListConsentTemplatesQuery, IReadOnlyList<ConsentTemplateDto>>
{
    public async Task<IReadOnlyList<ConsentTemplateDto>> Handle(ListConsentTemplatesQuery request, CancellationToken cancellationToken)
    {
        var templates = await repository.GetAllAsync(cancellationToken);
        return templates.Select(t => new ConsentTemplateDto(t.Id, t.TenantId, t.Purpose, t.Title,
            t.BodyText, t.VersionNumber, t.Status, t.PublishedAt, t.CreatedAt)).ToList();
    }
}
