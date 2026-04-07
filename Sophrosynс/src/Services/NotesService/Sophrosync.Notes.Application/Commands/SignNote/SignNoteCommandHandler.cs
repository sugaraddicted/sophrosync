using MediatR;
using Sophrosync.Notes.Application.DTOs;
using Sophrosync.Notes.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Notes.Application.Commands.SignNote;

public sealed class SignNoteCommandHandler(
    INoteRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<SignNoteCommand, NoteDto?>
{
    public async Task<NoteDto?> Handle(SignNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (note is null)
            return null;

        if (currentUser.IsInRole("therapist") && note.TherapistId != currentUser.Id)
            throw new UnauthorizedAccessException("You are not the owner of this note.");

        var signerFullName = currentUser.FullName;

        note.Sign(currentUser.Id, signerFullName, DateTime.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);

        return NoteDto.FromNote(note);
    }
}
