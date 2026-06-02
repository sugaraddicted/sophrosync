using MediatR;
using Sophrosync.Notes.Application.DTOs;
using Sophrosync.Notes.Domain.Entities;
using Sophrosync.Notes.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Notes.Application.Commands.AmendNote;

public sealed class AmendNoteCommandHandler(
    INoteRepository repository,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser)
    : IRequestHandler<AmendNoteCommand, NoteDto?>
{
    public async Task<NoteDto?> Handle(AmendNoteCommand request, CancellationToken cancellationToken)
    {
        if (!currentTenant.HasTenant)
            throw new InvalidOperationException("Tenant context is required to amend a note.");

        var original = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (original is null)
            return null;

        if (currentUser.IsInRole("therapist") && original.TherapistId != currentUser.Id)
            throw new UnauthorizedAccessException("You are not the owner of this note.");

        // Transition original note to Amended status
        original.Amend();

        var authorFullName = currentUser.FullName;

        // Create new Draft note referencing the original
        var amended = Note.Create(
            currentTenant.Id,
            original.ClientId,
            original.AppointmentId,
            currentUser.Id,
            authorFullName,
            original.Type,
            request.Title,
            request.Content,
            request.Tags,
            amendedFromId: original.Id);

        await repository.AddAsync(amended, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return NoteDto.FromNote(amended);
    }
}
