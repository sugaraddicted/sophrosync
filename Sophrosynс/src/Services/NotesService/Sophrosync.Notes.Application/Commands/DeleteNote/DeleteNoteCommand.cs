using MediatR;

namespace Sophrosync.Notes.Application.Commands.DeleteNote;

/// <summary>
/// Soft-deletes a Draft note. Locked and Signed notes cannot be deleted.
/// </summary>
public sealed record DeleteNoteCommand(Guid Id) : IRequest<bool>;
