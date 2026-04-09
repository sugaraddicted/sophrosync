using MediatR;
using Microsoft.Extensions.Logging;
using Sophrosync.Notes.Application.DTOs;
using Sophrosync.Notes.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Notes.Application.Queries.GetNoteById;

public sealed class GetNoteByIdQueryHandler(
    INoteRepository repository,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser,
    ILogger<GetNoteByIdQueryHandler> logger)
    : IRequestHandler<GetNoteByIdQuery, NoteDto?>
{
    public async Task<NoteDto?> Handle(GetNoteByIdQuery request, CancellationToken cancellationToken)
    {
        var note = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (note is not null)
        {
            // Structured PHI access log — never log the actual PHI content
            logger.LogInformation(
                "PHI access: note retrieved. TenantId={TenantId}, UserId={UserId}, NoteId={NoteId}, ClientId={ClientId}",
                currentTenant.Id,
                currentUser.Id,
                note.Id,
                note.ClientId);
        }

        return note is null ? null : NoteDto.FromNote(note);
    }
}
