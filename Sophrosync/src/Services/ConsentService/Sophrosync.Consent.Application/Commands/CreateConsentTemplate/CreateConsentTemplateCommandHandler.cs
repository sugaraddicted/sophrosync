using MediatR;
using Sophrosync.Consent.Domain.Entities;
using Sophrosync.Consent.Domain.Interfaces;

namespace Sophrosync.Consent.Application.Commands.CreateConsentTemplate;

public sealed class CreateConsentTemplateCommandHandler(
    IConsentTemplateRepository repository) : IRequestHandler<CreateConsentTemplateCommand, Guid>
{
    public async Task<Guid> Handle(CreateConsentTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = ConsentTemplate.Create(request.TenantId, request.Purpose, request.Title, request.BodyText);
        await repository.AddAsync(template, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return template.Id;
    }
}
