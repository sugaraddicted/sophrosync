using MediatR;
using Sophrosync.Notes.Application.DTOs;

namespace Sophrosync.Notes.Application.Commands.RequestCoSign;

/// <summary>
/// Transitions a Draft note to PendingCoSign status.
/// </summary>
public sealed record RequestCoSignCommand(Guid Id) : IRequest<NoteDto?>;
