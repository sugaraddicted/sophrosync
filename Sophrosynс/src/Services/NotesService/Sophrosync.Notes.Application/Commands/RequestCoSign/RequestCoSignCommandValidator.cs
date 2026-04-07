using FluentValidation;

namespace Sophrosync.Notes.Application.Commands.RequestCoSign;

public sealed class RequestCoSignCommandValidator : AbstractValidator<RequestCoSignCommand>
{
    public RequestCoSignCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Note id is required.");
    }
}
