using FluentValidation;

namespace Sophrosync.Notes.Application.Commands.SignNote;

public sealed class SignNoteCommandValidator : AbstractValidator<SignNoteCommand>
{
    public SignNoteCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Note id is required.");
    }
}
