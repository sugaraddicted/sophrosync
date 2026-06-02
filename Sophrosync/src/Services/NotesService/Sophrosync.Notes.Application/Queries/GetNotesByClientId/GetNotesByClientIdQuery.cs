using MediatR;
using Sophrosync.Notes.Application.DTOs;

namespace Sophrosync.Notes.Application.Queries.GetNotesByClientId;

/// <summary>
/// Returns all non-deleted notes for the specified client within the current tenant.
/// </summary>
public sealed record GetNotesByClientIdQuery(Guid ClientId) : IRequest<List<NoteDto>>;
