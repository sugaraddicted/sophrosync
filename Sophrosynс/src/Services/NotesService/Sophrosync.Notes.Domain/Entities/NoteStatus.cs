namespace Sophrosync.Notes.Domain.Entities;

/// <summary>
/// Defines the allowed lifecycle status constants for a clinical note.
/// </summary>
public static class NoteStatus
{
    public const string Draft = "Draft";
    public const string PendingCoSign = "PendingCoSign";
    public const string Signed = "Signed";
    public const string Locked = "Locked";
    public const string Amended = "Amended";
}
