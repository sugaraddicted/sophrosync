using Sophrosync.SharedKernel.Domain;

namespace Sophrosync.Notes.Domain.Entities;

/// <summary>
/// Aggregate root representing a clinical note within a tenant.
/// Title and Content are PHI — encrypted at the Infrastructure layer via EncryptedStringConverter.
/// </summary>
public sealed class Note : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid? AppointmentId { get; private set; }
    public Guid TherapistId { get; private set; }

    /// <summary>
    /// Denormalized therapist name captured at note creation time — never updated.
    /// </summary>
    public string AuthorFullName { get; private set; } = string.Empty;

    /// <summary>
    /// Note type constant from <see cref="NoteType"/>.
    /// </summary>
    public string Type { get; private set; } = string.Empty;

    /// <summary>
    /// PHI — encrypted at the Infrastructure layer.
    /// </summary>
    public string? Title { get; private set; }

    /// <summary>
    /// PHI — encrypted at the Infrastructure layer.
    /// </summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>
    /// Comma-separated tags. Not PHI.
    /// </summary>
    public string? Tags { get; private set; }

    /// <summary>
    /// Current lifecycle status from <see cref="NoteStatus"/>.
    /// </summary>
    public string Status { get; private set; } = NoteStatus.Draft;

    public DateTime? SignedAt { get; private set; }
    public Guid? SignedByUserId { get; private set; }

    /// <summary>
    /// Denormalized signer name captured at sign time.
    /// </summary>
    public string? SignedByFullName { get; private set; }

    public DateTime? LockedAt { get; private set; }
    public Guid? LockedByUserId { get; private set; }

    /// <summary>
    /// Denormalized locker name captured at lock time.
    /// </summary>
    public string? LockedByFullName { get; private set; }

    /// <summary>
    /// Reference to the original note when this note was created via amendment.
    /// </summary>
    public Guid? AmendedFromId { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedByUserId { get; private set; }
    public string? DeletedByFullName { get; private set; }

    private Note() { }

    /// <summary>
    /// Creates a new Note aggregate in Draft status.
    /// </summary>
    public static Note Create(
        Guid tenantId,
        Guid clientId,
        Guid? appointmentId,
        Guid therapistId,
        string authorFullName,
        string type,
        string? title,
        string content,
        string? tags,
        Guid? amendedFromId = null)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId must not be empty.", nameof(tenantId));

        if (clientId == Guid.Empty)
            throw new ArgumentException("ClientId must not be empty.", nameof(clientId));

        if (therapistId == Guid.Empty)
            throw new ArgumentException("TherapistId must not be empty.", nameof(therapistId));

        ArgumentException.ThrowIfNullOrWhiteSpace(authorFullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        if (!NoteType.IsValid(type))
            throw new ArgumentException($"Invalid note type: '{type}'.", nameof(type));

        return new Note
        {
            TenantId = tenantId,
            ClientId = clientId,
            AppointmentId = appointmentId,
            TherapistId = therapistId,
            AuthorFullName = authorFullName,
            Type = type,
            Title = title,
            Content = content,
            Tags = tags,
            Status = NoteStatus.Draft,
            AmendedFromId = amendedFromId
        };
    }

    /// <summary>
    /// Updates the mutable content fields of a Draft or PendingCoSign note.
    /// Locked and Amended notes cannot be updated.
    /// </summary>
    public void Update(string? title, string content, string? tags)
    {
        if (Status == NoteStatus.Locked || Status == NoteStatus.Amended)
            throw new InvalidOperationException($"Cannot update a note in '{Status}' status.");

        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        // Editing a note in PendingCoSign or Signed status withdraws/voids the signature,
        // since the content the signature attested to has changed.
        if (Status == NoteStatus.PendingCoSign || Status == NoteStatus.Signed)
            Status = NoteStatus.Draft;

        Title = title;
        Content = content;
        Tags = tags;
    }

    /// <summary>
    /// Transitions a Draft note to PendingCoSign, indicating it awaits co-signature.
    /// </summary>
    public void RequestCoSign()
    {
        if (Status != NoteStatus.Draft)
            throw new InvalidOperationException($"Cannot request co-sign from status '{Status}'. Note must be in Draft status.");

        Status = NoteStatus.PendingCoSign;
    }

    /// <summary>
    /// Signs a Draft or PendingCoSign note, transitioning it to Signed status.
    /// </summary>
    public void Sign(Guid signedByUserId, string signedByFullName, DateTime signedAt)
    {
        if (Status != NoteStatus.Draft && Status != NoteStatus.PendingCoSign)
            throw new InvalidOperationException($"Cannot sign a note in '{Status}' status. Note must be Draft or PendingCoSign.");

        Status = NoteStatus.Signed;
        SignedAt = signedAt;
        SignedByUserId = signedByUserId;
        SignedByFullName = signedByFullName;
    }

    /// <summary>
    /// Locks a Signed note, preventing further edits. Only a Signed note can be locked.
    /// </summary>
    public void Lock(Guid lockedByUserId, string lockedByFullName, DateTime lockedAt)
    {
        if (Status != NoteStatus.Signed)
            throw new InvalidOperationException($"Cannot lock a note in '{Status}' status. Note must be Signed.");

        Status = NoteStatus.Locked;
        LockedAt = lockedAt;
        LockedByUserId = lockedByUserId;
        LockedByFullName = lockedByFullName;
    }

    /// <summary>
    /// Marks this Locked note as Amended. The handler creates a new Draft note with AmendedFromId pointing here.
    /// </summary>
    public void Amend()
    {
        if (Status != NoteStatus.Locked)
            throw new InvalidOperationException($"Cannot amend a note in '{Status}' status. Note must be Locked.");

        Status = NoteStatus.Amended;
    }

    /// <summary>
    /// Soft-deletes a Draft note. Locked and Signed notes cannot be deleted.
    /// </summary>
    /// <param name="deletedAt">Explicit deletion timestamp — passed in by the caller for consistency with SaveChangesAsync audit timestamps.</param>
    /// <param name="deletedByUserId">Id of the user performing the deletion.</param>
    /// <param name="deletedByFullName">Denormalized full name of the user performing the deletion.</param>
    public void SoftDelete(DateTime deletedAt, Guid deletedByUserId, string deletedByFullName)
    {
        if (Status != NoteStatus.Draft)
            throw new InvalidOperationException($"Cannot delete a note in '{Status}' status. Only Draft notes may be deleted.");

        IsDeleted = true;
        DeletedAt = deletedAt;
        DeletedByUserId = deletedByUserId;
        DeletedByFullName = deletedByFullName;
    }
}
