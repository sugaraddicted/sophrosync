using FluentValidation;

namespace Sophrosync.Identity.Application.Commands.RegisterPractice;

public sealed class RegisterPracticeCommandValidator : AbstractValidator<RegisterPracticeCommand>
{
    public RegisterPracticeCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PracticeName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TimeZone).NotEmpty();
        RuleFor(x => x.AcceptedTerms)
            .Equal(true)
            .WithMessage("You must accept the Terms of Service and Business Associate Agreement.");
    }
}
