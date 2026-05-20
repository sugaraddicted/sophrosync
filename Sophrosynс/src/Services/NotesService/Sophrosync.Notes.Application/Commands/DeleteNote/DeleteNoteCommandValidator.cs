using FluentValidation;

namespace Sophrosync.Notes.Application.Commands.DeleteNote;

public sealed class DeleteNoteCommandValidator : AbstractValidator<DeleteNoteCommand>
{
    public DeleteNoteCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Note id is required.");
    }
}
