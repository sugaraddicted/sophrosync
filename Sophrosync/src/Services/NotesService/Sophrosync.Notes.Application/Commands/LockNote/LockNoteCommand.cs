using MediatR;
using Sophrosync.Notes.Application.DTOs;

namespace Sophrosync.Notes.Application.Commands.LockNote;

/// <summary>
/// Locks a Signed note, preventing further edits or signing.
/// </summary>
public sealed record LockNoteCommand(Guid Id) : IRequest<NoteDto?>;
