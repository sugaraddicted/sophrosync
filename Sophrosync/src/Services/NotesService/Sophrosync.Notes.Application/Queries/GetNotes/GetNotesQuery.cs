using MediatR;
using Sophrosync.Notes.Application.DTOs;

namespace Sophrosync.Notes.Application.Queries.GetNotes;

/// <summary>
/// Returns all non-deleted notes visible to the current tenant.
/// </summary>
public sealed record GetNotesQuery() : IRequest<List<NoteDto>>;
