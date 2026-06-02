using FluentValidation;

namespace Sophrosync.Notes.Application.Commands.LockNote;

public sealed class LockNoteCommandValidator : AbstractValidator<LockNoteCommand>
{
    public LockNoteCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Note id is required.");
    }
}
