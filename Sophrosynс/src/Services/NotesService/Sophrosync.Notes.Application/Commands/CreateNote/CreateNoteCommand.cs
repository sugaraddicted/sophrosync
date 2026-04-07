using MediatR;
using Sophrosync.Notes.Application.DTOs;

namespace Sophrosync.Notes.Application.Commands.CreateNote;

/// <summary>
/// Creates a new clinical note within the current tenant.
/// TenantId and TherapistId are set by the handler from ICurrentTenant/ICurrentUser — never supplied by the caller.
/// </summary>
public sealed record CreateNoteCommand(
    Guid ClientId,
    Guid? AppointmentId,
    string Type,
    string? Title,
    string Content,
    string? Tags) : IRequest<NoteDto>;
