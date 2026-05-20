using FluentValidation;
using Sophrosync.Clients.Application.Commands.CreateClient;

namespace Sophrosync.Clients.Application.Validators;

public sealed class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(320)
            .EmailAddress();

        RuleFor(x => x.Phone)
            .MaximumLength(50);
    }
}
