using MediatR;
using Sophrosync.Notes.Application.DTOs;

namespace Sophrosync.Notes.Application.Queries.GetNoteById;

/// <summary>
/// Returns a single clinical note by its Id.
/// </summary>
public sealed record GetNoteByIdQuery(Guid Id) : IRequest<NoteDto?>;
