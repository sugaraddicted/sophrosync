using MediatR;
using Sophrosync.Notes.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Notes.Application.Commands.DeleteNote;

public sealed class DeleteNoteCommandHandler(
    INoteRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<DeleteNoteCommand, bool>
{
    public async Task<bool> Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (note is null)
            return false;

        if (currentUser.IsInRole("therapist") && note.TherapistId != currentUser.Id)
            throw new UnauthorizedAccessException("You are not the owner of this note.");

        note.SoftDelete(DateTime.UtcNow, currentUser.Id, currentUser.FullName);
        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
