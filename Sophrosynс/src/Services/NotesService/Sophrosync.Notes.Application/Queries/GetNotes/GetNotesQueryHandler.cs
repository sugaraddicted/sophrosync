using MediatR;
using Microsoft.Extensions.Logging;
using Sophrosync.Notes.Application.DTOs;
using Sophrosync.Notes.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Notes.Application.Queries.GetNotes;

public sealed class GetNotesQueryHandler(
    INoteRepository repository,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser,
    ILogger<GetNotesQueryHandler> logger)
    : IRequestHandler<GetNotesQuery, List<NoteDto>>
{
    public async Task<List<NoteDto>> Handle(GetNotesQuery request, CancellationToken cancellationToken)
    {
        var notes = await repository.GetAllAsync(cancellationToken);

        // Structured PHI access log — never log the actual PHI content
        logger.LogInformation(
            "PHI access: notes list retrieved. TenantId={TenantId}, UserId={UserId}, NoteCount={NoteCount}",
            currentTenant.Id,
            currentUser.Id,
            notes.Count);

        return notes.Select(NoteDto.FromNote).ToList();
    }
}
