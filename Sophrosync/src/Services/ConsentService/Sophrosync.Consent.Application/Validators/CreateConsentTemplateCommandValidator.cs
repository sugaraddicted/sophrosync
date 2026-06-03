using FluentValidation;
using Sophrosync.Consent.Application.Commands.CreateConsentTemplate;

namespace Sophrosync.Consent.Application.Validators;

public sealed class CreateConsentTemplateCommandValidator : AbstractValidator<CreateConsentTemplateCommand>
{
    public CreateConsentTemplateCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        RuleFor(x => x.BodyText).NotEmpty().MaximumLength(50000);
    }
}
