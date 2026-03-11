using MediatR;
using Sophrosync.Consent.Domain.Interfaces;

namespace Sophrosync.Consent.Application.Commands.RetireConsentTemplate;

public sealed class RetireConsentTemplateCommandHandler(
    IConsentTemplateRepository repository) : IRequestHandler<RetireConsentTemplateCommand>
{
    public async Task Handle(RetireConsentTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await repository.GetByIdAsync(request.TemplateId, cancellationToken)
            ?? throw new InvalidOperationException($"ConsentTemplate {request.TemplateId} not found.");
        template.Retire();
        repository.Update(template);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
