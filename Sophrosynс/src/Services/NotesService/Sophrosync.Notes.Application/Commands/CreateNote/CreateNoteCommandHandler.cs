using MediatR;
using Sophrosync.Notes.Application.DTOs;
using Sophrosync.Notes.Domain.Entities;
using Sophrosync.Notes.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Notes.Application.Commands.CreateNote;

public sealed class CreateNoteCommandHandler(
    INoteRepository repository,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser)
    : IRequestHandler<CreateNoteCommand, NoteDto>
{
    public async Task<NoteDto> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
    {
        if (!currentTenant.HasTenant)
            throw new InvalidOperationException(
                "tenant_id claim is missing from the JWT. " +
                "Add the tenant_id user attribute to this account in Keycloak and ensure the tenant-id-claim mapper is configured on the client.");

        var authorFullName = currentUser.FullName;

        var note = Note.Create(
            currentTenant.Id,
            request.ClientId,
            request.AppointmentId,
            currentUser.Id,
            authorFullName,
            request.Type,
            request.Title,
            request.Content,
            request.Tags);

        await repository.AddAsync(note, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return NoteDto.FromNote(note);
    }
}
