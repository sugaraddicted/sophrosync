using MediatR;
using Sophrosync.Notes.Application.DTOs;

namespace Sophrosync.Notes.Application.Commands.AmendNote;

/// <summary>
/// Amends a Locked note by transitioning it to Amended and creating a new Draft note.
/// Returns the DTO for the newly created Draft note.
/// </summary>
public sealed record AmendNoteCommand(
    Guid Id,
    string? Title,
    string Content,
    string? Tags) : IRequest<NoteDto?>;
