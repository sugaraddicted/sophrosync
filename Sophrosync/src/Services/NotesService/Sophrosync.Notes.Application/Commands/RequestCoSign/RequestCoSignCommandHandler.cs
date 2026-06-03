using MediatR;
using Sophrosync.Notes.Application.DTOs;
using Sophrosync.Notes.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Notes.Application.Commands.RequestCoSign;

public sealed class RequestCoSignCommandHandler(
    INoteRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<RequestCoSignCommand, NoteDto?>
{
    public async Task<NoteDto?> Handle(RequestCoSignCommand request, CancellationToken cancellationToken)
    {
        var note = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (note is null)
            return null;

        if (currentUser.IsInRole("therapist") && note.TherapistId != currentUser.Id)
            throw new UnauthorizedAccessException("You are not the owner of this note.");

        note.RequestCoSign();
        await repository.SaveChangesAsync(cancellationToken);

        return NoteDto.FromNote(note);
    }
}
