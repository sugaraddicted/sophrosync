using FluentValidation;
using Sophrosync.Consent.Application.Commands.IssueConsentRequest;

namespace Sophrosync.Consent.Application.Validators;

public sealed class IssueConsentRequestCommandValidator : AbstractValidator<IssueConsentRequestCommand>
{
    public IssueConsentRequestCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.ConsentTemplateId).NotEmpty();
        RuleFor(x => x.ExpiresAt).GreaterThan(DateTime.UtcNow)
            .WithMessage("ExpiresAt must be in the future.");
    }
}
