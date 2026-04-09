using MediatR;
using Sophrosync.Notes.Application.DTOs;
using Sophrosync.Notes.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Notes.Application.Commands.LockNote;

public sealed class LockNoteCommandHandler(
    INoteRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<LockNoteCommand, NoteDto?>
{
    public async Task<NoteDto?> Handle(LockNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (note is null)
            return null;

        if (currentUser.IsInRole("therapist") && note.TherapistId != currentUser.Id)
            throw new UnauthorizedAccessException("You are not the owner of this note.");

        var lockerFullName = currentUser.FullName;

        note.Lock(currentUser.Id, lockerFullName, DateTime.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);

        return NoteDto.FromNote(note);
    }
}
