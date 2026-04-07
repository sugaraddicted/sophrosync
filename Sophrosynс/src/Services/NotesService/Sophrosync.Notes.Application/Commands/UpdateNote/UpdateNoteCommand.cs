using MediatR;
using Sophrosync.Notes.Application.DTOs;

namespace Sophrosync.Notes.Application.Commands.UpdateNote;

/// <summary>
/// Updates the mutable content fields of an existing clinical note.
/// Only Draft and PendingCoSign notes may be updated.
/// </summary>
public sealed record UpdateNoteCommand(
    Guid Id,
    string? Title,
    string Content,
    string? Tags) : IRequest<NoteDto?>;
