using Sophrosync.Notes.Domain.Entities;

namespace Sophrosync.Notes.Application.DTOs;

/// <summary>
/// Read model returned to callers for a single clinical note.
/// </summary>
public sealed record NoteDto(
    Guid Id,
    Guid ClientId,
    Guid? AppointmentId,
    Guid TherapistId,
    string AuthorFullName,
    string Type,
    string? Title,
    string Content,
    string? Tags,
    string Status,
    DateTime? SignedAt,
    Guid? SignedByUserId,
    string? SignedByFullName,
    DateTime? LockedAt,
    Guid? LockedByUserId,
    string? LockedByFullName,
    Guid? AmendedFromId,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    /// <summary>
    /// Maps a <see cref="Note"/> aggregate to a <see cref="NoteDto"/>.
    /// </summary>
    public static NoteDto FromNote(Note note) => new(
        note.Id,
        note.ClientId,
        note.AppointmentId,
        note.TherapistId,
        note.AuthorFullName,
        note.Type,
        note.Title,
        note.Content,
        note.Tags,
        note.Status,
        note.SignedAt,
        note.SignedByUserId,
        note.SignedByFullName,
        note.LockedAt,
        note.LockedByUserId,
        note.LockedByFullName,
        note.AmendedFromId,
        note.CreatedAt,
        note.UpdatedAt);
}
