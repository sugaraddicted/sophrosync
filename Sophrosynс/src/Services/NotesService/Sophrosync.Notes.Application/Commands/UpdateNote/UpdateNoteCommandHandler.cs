using MediatR;
using Sophrosync.Notes.Application.DTOs;
using Sophrosync.Notes.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Notes.Application.Commands.UpdateNote;

public sealed class UpdateNoteCommandHandler(
    INoteRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<UpdateNoteCommand, NoteDto?>
{
    public async Task<NoteDto?> Handle(UpdateNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (note is null)
            return null;

        if (currentUser.IsInRole("therapist") && note.TherapistId != currentUser.Id)
            throw new UnauthorizedAccessException("You are not the owner of this note.");

        note.Update(request.Title, request.Content, request.Tags);
        await repository.SaveChangesAsync(cancellationToken);

        return NoteDto.FromNote(note);
    }
}
