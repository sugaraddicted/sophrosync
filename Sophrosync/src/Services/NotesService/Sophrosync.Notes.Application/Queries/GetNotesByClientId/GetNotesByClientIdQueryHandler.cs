using MediatR;
using Microsoft.Extensions.Logging;
using Sophrosync.Notes.Application.DTOs;
using Sophrosync.Notes.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Notes.Application.Queries.GetNotesByClientId;

public sealed class GetNotesByClientIdQueryHandler(
    INoteRepository repository,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser,
    ILogger<GetNotesByClientIdQueryHandler> logger)
    : IRequestHandler<GetNotesByClientIdQuery, List<NoteDto>>
{
    public async Task<List<NoteDto>> Handle(GetNotesByClientIdQuery request, CancellationToken cancellationToken)
    {
        var notes = await repository.GetByClientIdAsync(request.ClientId, cancellationToken);

        // Structured PHI access log — never log the actual PHI content
        logger.LogInformation(
            "PHI access: notes list retrieved by client. TenantId={TenantId}, UserId={UserId}, ClientId={ClientId}, NoteCount={NoteCount}",
            currentTenant.Id,
            currentUser.Id,
            request.ClientId,
            notes.Count);

        return notes.Select(NoteDto.FromNote).ToList();
    }
}
