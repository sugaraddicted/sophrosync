using FluentValidation;
using Sophrosync.Clients.Application.Commands.UpdateClient;
using Sophrosync.Clients.Domain.Entities;

namespace Sophrosync.Clients.Application.Validators;

public sealed class UpdateClientCommandValidator : AbstractValidator<UpdateClientCommand>
{
    public UpdateClientCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(320)
            .EmailAddress();

        RuleFor(x => x.Phone)
            .MaximumLength(50);

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => s == ClientStatus.Active || s == ClientStatus.Inactive)
            .WithMessage($"Status must be '{ClientStatus.Active}' or '{ClientStatus.Inactive}'.");
    }
}
