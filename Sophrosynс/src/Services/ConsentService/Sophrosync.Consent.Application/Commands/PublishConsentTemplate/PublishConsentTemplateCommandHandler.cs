using MediatR;
using Sophrosync.Consent.Domain.Interfaces;

namespace Sophrosync.Consent.Application.Commands.PublishConsentTemplate;

public sealed class PublishConsentTemplateCommandHandler(
    IConsentTemplateRepository repository,
    IPublisher publisher) : IRequestHandler<PublishConsentTemplateCommand>
{
    public async Task Handle(PublishConsentTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await repository.GetByIdAsync(request.TemplateId, cancellationToken)
            ?? throw new InvalidOperationException($"ConsentTemplate {request.TemplateId} not found.");
        template.Publish();
        repository.Update(template);
        await repository.SaveChangesAsync(cancellationToken);
        foreach (var evt in template.DomainEvents) await publisher.Publish(evt, cancellationToken);
        template.ClearDomainEvents();
    }
}
