using FluentAssertions;
using Sophrosync.Notes.Domain.Entities;

namespace Sophrosync.Notes.Tests;

/// <summary>
/// Pure domain unit tests for the Note aggregate. No mocks required.
/// </summary>
public sealed class NoteEntityTests
{
    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static readonly Guid TenantId    = Guid.NewGuid();
    private static readonly Guid ClientId    = Guid.NewGuid();
    private static readonly Guid TherapistId = Guid.NewGuid();
    private static readonly Guid UserId      = Guid.NewGuid();

    private static Note CreateValidDraftNote(
        Guid? tenantId    = null,
        Guid? clientId    = null,
        Guid? therapistId = null) =>
        Note.Create(
            tenantId    ?? TenantId,
            clientId    ?? ClientId,
            appointmentId: null,
            therapistId ?? TherapistId,
            authorFullName: "Dr. Jane Smith",
            type: NoteType.DAP,
            title: "Session title",
            content: "Session content.",
            tags: null);

    private static Note CreateSignedNote()
    {
        var note = CreateValidDraftNote();
        note.Sign(UserId, "Dr. Jane Smith", DateTime.UtcNow);
        return note;
    }

    private static Note CreateLockedNote()
    {
        var note = CreateSignedNote();
        note.Lock(UserId, "Dr. Jane Smith", DateTime.UtcNow);
        return note;
    }

    // -----------------------------------------------------------------------
    // Create — happy paths
    // -----------------------------------------------------------------------

    [Fact]
    public void Create_WithValidArgs_ReturnsDraftNote()
    {
        var note = CreateValidDraftNote();

        note.TenantId.Should().Be(TenantId);
        note.ClientId.Should().Be(ClientId);
        note.TherapistId.Should().Be(TherapistId);
        note.AuthorFullName.Should().Be("Dr. Jane Smith");
        note.Type.Should().Be(NoteType.DAP);
        note.Title.Should().Be("Session title");
        note.Content.Should().Be("Session content.");
        note.Status.Should().Be(NoteStatus.Draft);
        note.IsDeleted.Should().BeFalse();
        note.AmendedFromId.Should().BeNull();
        note.Id.Should().NotBeEmpty();
    }

    // -----------------------------------------------------------------------
    // Create — argument guard tests
    // -----------------------------------------------------------------------

    [Fact]
    public void Create_WithEmptyTenantId_ThrowsArgumentException()
    {
        var act = () => CreateValidDraftNote(tenantId: Guid.Empty);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*TenantId*");
    }

    [Fact]
    public void Create_WithEmptyClientId_ThrowsArgumentException()
    {
        var act = () => Note.Create(
            TenantId, Guid.Empty, null, TherapistId,
            "Author", NoteType.DAP, null, "content", null);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*ClientId*");
    }

    [Fact]
    public void Create_WithEmptyTherapistId_ThrowsArgumentException()
    {
        var act = () => CreateValidDraftNote(therapistId: Guid.Empty);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*TherapistId*");
    }

    [Fact]
    public void Create_WithInvalidNoteType_ThrowsArgumentException()
    {
        var act = () => Note.Create(
            TenantId, ClientId, null, TherapistId,
            "Author", "InvalidType", null, "content", null);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid note type*");
    }

    // -----------------------------------------------------------------------
    // Update — happy paths
    // -----------------------------------------------------------------------

    [Fact]
    public void Update_DraftNote_UpdatesContentAndTitle()
    {
        var note = CreateValidDraftNote();

        note.Update("New title", "New content.", "tag1,tag2");

        note.Title.Should().Be("New title");
        note.Content.Should().Be("New content.");
        note.Tags.Should().Be("tag1,tag2");
    }

    [Fact]
    public void Update_PendingCoSignNote_ResetsStatusToDraft()
    {
        var note = CreateValidDraftNote();
        note.RequestCoSign();
        note.Status.Should().Be(NoteStatus.PendingCoSign);

        note.Update(null, "Updated content.", null);

        note.Status.Should().Be(NoteStatus.Draft);
        note.Content.Should().Be("Updated content.");
    }

    // -----------------------------------------------------------------------
    // Update — invalid state transitions
    // -----------------------------------------------------------------------

    [Fact]
    public void Update_LockedNote_ThrowsInvalidOperationException()
    {
        var note = CreateLockedNote();

        var act = () => note.Update(null, "Attempt to update.", null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*'{NoteStatus.Locked}'*");
    }

    [Fact]
    public void Update_SignedNote_SucceedsAndResetsStatusToDraft()
    {
        // Editing a Signed note voids the signature: the content the signature attested to
        // has changed, so the note returns to Draft and must be re-signed.
        var note = CreateSignedNote();

        note.Update(null, "Updated after sign.", null);

        note.Content.Should().Be("Updated after sign.");
        note.Status.Should().Be(NoteStatus.Draft);
    }

    [Fact]
    public void Update_AmendedNote_ThrowsInvalidOperationException()
    {
        var note = CreateLockedNote();
        note.Amend();

        var act = () => note.Update(null, "Attempt to update.", null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*'{NoteStatus.Amended}'*");
    }

    // -----------------------------------------------------------------------
    // Sign — happy path
    // -----------------------------------------------------------------------

    [Fact]
    public void Sign_DraftNote_SetsStatusSigned_AndRecordsSignatureInfo()
    {
        var note     = CreateValidDraftNote();
        var signedAt = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        note.Sign(UserId, "Dr. Jane Smith", signedAt);

        note.Status.Should().Be(NoteStatus.Signed);
        note.SignedAt.Should().Be(signedAt);
        note.SignedByUserId.Should().Be(UserId);
        note.SignedByFullName.Should().Be("Dr. Jane Smith");
    }

    [Fact]
    public void Sign_RecordsCorrectSignerIdAndName()
    {
        var note      = CreateValidDraftNote();
        var signerId  = Guid.NewGuid();
        var signerName = "Dr. Alice Jones";

        note.Sign(signerId, signerName, DateTime.UtcNow);

        note.SignedByUserId.Should().Be(signerId);
        note.SignedByFullName.Should().Be(signerName);
    }

    [Fact]
    public void Sign_PendingCoSignNote_SetsStatusSigned()
    {
        var note = CreateValidDraftNote();
        note.RequestCoSign();

        note.Sign(UserId, "Dr. Jane Smith", DateTime.UtcNow);

        note.Status.Should().Be(NoteStatus.Signed);
    }

    // -----------------------------------------------------------------------
    // Sign — invalid state transitions
    // -----------------------------------------------------------------------

    [Fact]
    public void Sign_AlreadySignedNote_ThrowsInvalidOperationException()
    {
        var note = CreateSignedNote();

        var act = () => note.Sign(UserId, "Dr. Jane Smith", DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*'{NoteStatus.Signed}'*");
    }

    [Fact]
    public void Sign_LockedNote_ThrowsInvalidOperationException()
    {
        var note = CreateLockedNote();

        var act = () => note.Sign(UserId, "Dr. Jane Smith", DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*'{NoteStatus.Locked}'*");
    }

    // -----------------------------------------------------------------------
    // Lock — happy path
    // -----------------------------------------------------------------------

    [Fact]
    public void Lock_SignedNote_SetsStatusLocked()
    {
        var note     = CreateSignedNote();
        var lockedAt = new DateTime(2025, 6, 2, 9, 0, 0, DateTimeKind.Utc);

        note.Lock(UserId, "Dr. Jane Smith", lockedAt);

        note.Status.Should().Be(NoteStatus.Locked);
        note.LockedAt.Should().Be(lockedAt);
        note.LockedByUserId.Should().Be(UserId);
        note.LockedByFullName.Should().Be("Dr. Jane Smith");
    }

    // -----------------------------------------------------------------------
    // Lock — invalid state transitions
    // -----------------------------------------------------------------------

    [Fact]
    public void Lock_DraftNote_ThrowsInvalidOperationException()
    {
        var note = CreateValidDraftNote();

        var act = () => note.Lock(UserId, "Dr. Jane Smith", DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*'{NoteStatus.Draft}'*");
    }

    [Fact]
    public void Lock_AlreadyLockedNote_ThrowsInvalidOperationException()
    {
        var note = CreateLockedNote();

        var act = () => note.Lock(UserId, "Dr. Jane Smith", DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*'{NoteStatus.Locked}'*");
    }

    // -----------------------------------------------------------------------
    // RequestCoSign — happy path
    // -----------------------------------------------------------------------

    [Fact]
    public void RequestCoSign_DraftNote_SetsStatusPendingCoSign()
    {
        var note = CreateValidDraftNote();

        note.RequestCoSign();

        note.Status.Should().Be(NoteStatus.PendingCoSign);
    }

    // -----------------------------------------------------------------------
    // RequestCoSign — invalid state transitions
    // -----------------------------------------------------------------------

    [Fact]
    public void RequestCoSign_LockedNote_ThrowsInvalidOperationException()
    {
        var note = CreateLockedNote();

        var act = () => note.RequestCoSign();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*'{NoteStatus.Locked}'*");
    }

    [Fact]
    public void RequestCoSign_SignedNote_ThrowsInvalidOperationException()
    {
        var note = CreateSignedNote();

        var act = () => note.RequestCoSign();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*'{NoteStatus.Signed}'*");
    }

    // -----------------------------------------------------------------------
    // Amend — happy path
    // -----------------------------------------------------------------------

    [Fact]
    public void Amend_LockedNote_SetsStatusAmended()
    {
        var note = CreateLockedNote();

        note.Amend();

        note.Status.Should().Be(NoteStatus.Amended);
    }

    // -----------------------------------------------------------------------
    // Amend — invalid state transitions
    // -----------------------------------------------------------------------

    [Fact]
    public void Amend_DraftNote_ThrowsInvalidOperationException()
    {
        var note = CreateValidDraftNote();

        var act = () => note.Amend();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*'{NoteStatus.Draft}'*");
    }

    [Fact]
    public void Amend_SignedNote_ThrowsInvalidOperationException()
    {
        var note = CreateSignedNote();

        var act = () => note.Amend();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*'{NoteStatus.Signed}'*");
    }

    // -----------------------------------------------------------------------
    // SoftDelete — happy path
    // -----------------------------------------------------------------------

    [Fact]
    public void SoftDelete_SetsIsDeletedTrue_AndRecordsAuditFields()
    {
        var note      = CreateValidDraftNote();
        var deletedAt = new DateTime(2025, 6, 3, 10, 0, 0, DateTimeKind.Utc);

        note.SoftDelete(deletedAt, UserId, "Dr. Jane Smith");

        note.IsDeleted.Should().BeTrue();
        note.DeletedAt.Should().Be(deletedAt);
        note.DeletedByUserId.Should().Be(UserId);
        note.DeletedByFullName.Should().Be("Dr. Jane Smith");
    }

    // -----------------------------------------------------------------------
    // SoftDelete — invalid state transitions
    // -----------------------------------------------------------------------

    [Fact]
    public void SoftDelete_SignedNote_ThrowsInvalidOperationException()
    {
        var note = CreateSignedNote();

        var act = () => note.SoftDelete(DateTime.UtcNow, UserId, "Dr. Jane Smith");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*'{NoteStatus.Signed}'*");
    }

    [Fact]
    public void SoftDelete_LockedNote_ThrowsInvalidOperationException()
    {
        var note = CreateLockedNote();

        var act = () => note.SoftDelete(DateTime.UtcNow, UserId, "Dr. Jane Smith");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*'{NoteStatus.Locked}'*");
    }
}
