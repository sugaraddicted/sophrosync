using FluentValidation;

namespace Sophrosync.Notes.Application.Commands.UpdateNote;

public sealed class UpdateNoteCommandValidator : AbstractValidator<UpdateNoteCommand>
{
    public UpdateNoteCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(50000);

        RuleFor(x => x.Title)
            .MaximumLength(200)
            .When(x => x.Title is not null);

        RuleFor(x => x.Tags)
            .MaximumLength(500)
            .Matches(@"^[\w ,\-]*$")
            .WithMessage("Tags may only contain letters, digits, spaces, commas, and hyphens.")
            .When(x => x.Tags is not null);
    }
}
