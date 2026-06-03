using MediatR;
using Sophrosync.Notes.Application.DTOs;

namespace Sophrosync.Notes.Application.Commands.SignNote;

/// <summary>
/// Signs a Draft or PendingCoSign note, transitioning it to Signed status.
/// </summary>
public sealed record SignNoteCommand(Guid Id) : IRequest<NoteDto?>;
