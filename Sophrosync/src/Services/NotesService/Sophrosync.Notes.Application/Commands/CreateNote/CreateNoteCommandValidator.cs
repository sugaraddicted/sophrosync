using FluentValidation;
using Sophrosync.Notes.Domain.Entities;

namespace Sophrosync.Notes.Application.Commands.CreateNote;

public sealed class CreateNoteCommandValidator : AbstractValidator<CreateNoteCommand>
{
    public CreateNoteCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty();

        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(NoteType.IsValid)
            .WithMessage($"Type must be one of: DAP, SOAP, FreeForm, Intake, Treatment, Discharge.");

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
