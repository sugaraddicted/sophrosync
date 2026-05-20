using FluentAssertions;
using Moq;
using Sophrosync.Notes.Application.Commands.AmendNote;
using Sophrosync.Notes.Application.Commands.CreateNote;
using Sophrosync.Notes.Application.Commands.DeleteNote;
using Sophrosync.Notes.Application.Commands.LockNote;
using Sophrosync.Notes.Application.Commands.RequestCoSign;
using Sophrosync.Notes.Application.Commands.SignNote;
using Sophrosync.Notes.Application.Commands.UpdateNote;
using Sophrosync.Notes.Domain.Entities;
using Sophrosync.Notes.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Notes.Tests;

/// <summary>
/// Unit tests for command handlers. All I/O dependencies are mocked.
/// </summary>
public sealed class CommandHandlerTests
{
    // -----------------------------------------------------------------------
    // Shared helpers
    // -----------------------------------------------------------------------

    private static readonly Guid TenantId    = Guid.NewGuid();
    private static readonly Guid TherapistId = Guid.NewGuid();
    private static readonly Guid ClientId    = Guid.NewGuid();
    private static readonly Guid AdminId     = Guid.NewGuid();

    /// <summary>
    /// Creates a therapist ICurrentUser mock — IsInRole("therapist") returns true.
    /// </summary>
    private static Mock<ICurrentUser> TherapistUser(Guid? userId = null)
    {
        var mock = new Mock<ICurrentUser>();
        mock.Setup(u => u.Id).Returns(userId ?? TherapistId);
        mock.Setup(u => u.FullName).Returns("Dr. Jane Smith");
        mock.Setup(u => u.IsInRole("therapist")).Returns(true);
        mock.Setup(u => u.IsInRole(It.Is<string>(r => r != "therapist"))).Returns(false);
        return mock;
    }

    /// <summary>
    /// Creates an admin ICurrentUser mock — IsInRole never returns true for "therapist".
    /// </summary>
    private static Mock<ICurrentUser> AdminUser()
    {
        var mock = new Mock<ICurrentUser>();
        mock.Setup(u => u.Id).Returns(AdminId);
        mock.Setup(u => u.FullName).Returns("Admin User");
        mock.Setup(u => u.IsInRole(It.IsAny<string>())).Returns(false);
        return mock;
    }

    private static Mock<ICurrentTenant> ValidTenant()
    {
        var mock = new Mock<ICurrentTenant>();
        mock.Setup(t => t.Id).Returns(TenantId);
        mock.Setup(t => t.HasTenant).Returns(true);
        return mock;
    }

    private static Mock<ICurrentTenant> NoTenant()
    {
        var mock = new Mock<ICurrentTenant>();
        mock.Setup(t => t.HasTenant).Returns(false);
        mock.Setup(t => t.Id).Returns(Guid.Empty);
        return mock;
    }

    private static Note BuildDraftNote(Guid? therapistId = null) =>
        Note.Create(
            TenantId,
            ClientId,
            appointmentId: null,
            therapistId ?? TherapistId,
            "Dr. Jane Smith",
            NoteType.DAP,
            "Title",
            "Content",
            tags: null);

    private static Note BuildSignedNote(Guid? therapistId = null)
    {
        var note = BuildDraftNote(therapistId);
        note.Sign(TherapistId, "Dr. Jane Smith", DateTime.UtcNow);
        return note;
    }

    private static Note BuildLockedNote(Guid? therapistId = null)
    {
        var note = BuildSignedNote(therapistId);
        note.Lock(TherapistId, "Dr. Jane Smith", DateTime.UtcNow);
        return note;
    }

    // -----------------------------------------------------------------------
    // CreateNoteCommandHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateHandler_ValidCommand_CreatesAndPersistsNote()
    {
        var repo    = new Mock<INoteRepository>();
        var tenant  = ValidTenant();
        var user    = TherapistUser();

        repo.Setup(r => r.AddAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Note n, CancellationToken _) => n);

        var handler = new CreateNoteCommandHandler(repo.Object, tenant.Object, user.Object);
        var cmd     = new CreateNoteCommand(ClientId, null, NoteType.SOAP, "T", "Content", null);

        var dto = await handler.Handle(cmd, CancellationToken.None);

        dto.Should().NotBeNull();
        dto.Status.Should().Be(NoteStatus.Draft);
        dto.ClientId.Should().Be(ClientId);
        dto.TherapistId.Should().Be(TherapistId);
        repo.Verify(r => r.AddAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateHandler_NoTenantContext_ThrowsInvalidOperationException()
    {
        var repo   = new Mock<INoteRepository>();
        var tenant = NoTenant();
        var user   = TherapistUser();

        var handler = new CreateNoteCommandHandler(repo.Object, tenant.Object, user.Object);
        var cmd     = new CreateNoteCommand(ClientId, null, NoteType.DAP, null, "Content", null);

        var act = () => handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*tenant_id*");
    }

    [Fact]
    public async Task CreateHandler_RepositoryThrows_PropagatesException()
    {
        var repo   = new Mock<INoteRepository>();
        var tenant = ValidTenant();
        var user   = TherapistUser();

        repo.Setup(r => r.AddAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var handler = new CreateNoteCommandHandler(repo.Object, tenant.Object, user.Object);
        var cmd     = new CreateNoteCommand(ClientId, null, NoteType.DAP, null, "Content", null);

        var act = () => handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*DB error*");
    }

    // -----------------------------------------------------------------------
    // UpdateNoteCommandHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task UpdateHandler_ValidCommand_AsOwnerTherapist_UpdatesNote()
    {
        var note = BuildDraftNote();
        var repo = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);

        var user    = TherapistUser(TherapistId);
        var handler = new UpdateNoteCommandHandler(repo.Object, user.Object);
        var cmd     = new UpdateNoteCommand(note.Id, "New title", "New content.", null);

        var dto = await handler.Handle(cmd, CancellationToken.None);

        dto.Should().NotBeNull();
        dto!.Title.Should().Be("New title");
        dto.Content.Should().Be("New content.");
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateHandler_NoteNotFound_ReturnsNull()
    {
        var repo = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Note?)null);

        var user    = TherapistUser();
        var handler = new UpdateNoteCommandHandler(repo.Object, user.Object);
        var cmd     = new UpdateNoteCommand(Guid.NewGuid(), null, "Content", null);

        var dto = await handler.Handle(cmd, CancellationToken.None);

        dto.Should().BeNull();
    }

    [Fact]
    public async Task UpdateHandler_TherapistNotOwner_ThrowsUnauthorizedAccessException()
    {
        var differentTherapist = Guid.NewGuid();
        var note = BuildDraftNote(therapistId: differentTherapist);
        var repo = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);

        // Current user is a therapist but NOT the note owner
        var user    = TherapistUser(TherapistId);
        var handler = new UpdateNoteCommandHandler(repo.Object, user.Object);
        var cmd     = new UpdateNoteCommand(note.Id, null, "Content", null);

        var act = () => handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not the owner*");
    }

    [Fact]
    public async Task UpdateHandler_AdminUser_CanUpdateAnyNote()
    {
        var note = BuildDraftNote(therapistId: Guid.NewGuid()); // owned by someone else
        var repo = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);

        var user    = AdminUser(); // not a therapist — ownership check skipped
        var handler = new UpdateNoteCommandHandler(repo.Object, user.Object);
        var cmd     = new UpdateNoteCommand(note.Id, null, "Admin override.", null);

        var dto = await handler.Handle(cmd, CancellationToken.None);

        dto.Should().NotBeNull();
        dto!.Content.Should().Be("Admin override.");
    }

    [Fact]
    public async Task UpdateHandler_PendingCoSignNote_ResetsStatusToDraft()
    {
        var note = BuildDraftNote();
        note.RequestCoSign();
        note.Status.Should().Be(NoteStatus.PendingCoSign);

        var repo = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);

        var user    = TherapistUser(TherapistId);
        var handler = new UpdateNoteCommandHandler(repo.Object, user.Object);
        var cmd     = new UpdateNoteCommand(note.Id, null, "Updated.", null);

        var dto = await handler.Handle(cmd, CancellationToken.None);

        dto!.Status.Should().Be(NoteStatus.Draft);
    }

    // -----------------------------------------------------------------------
    // DeleteNoteCommandHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task DeleteHandler_ValidCommand_SoftDeletesNote_WithAuditFields()
    {
        var note = BuildDraftNote();
        var repo = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);

        var user    = TherapistUser(TherapistId);
        var handler = new DeleteNoteCommandHandler(repo.Object, user.Object);

        var result = await handler.Handle(new DeleteNoteCommand(note.Id), CancellationToken.None);

        result.Should().BeTrue();
        note.IsDeleted.Should().BeTrue();
        note.DeletedByUserId.Should().Be(TherapistId);
        note.DeletedByFullName.Should().Be("Dr. Jane Smith");
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteHandler_NoteNotFound_ReturnsFalse()
    {
        var repo = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Note?)null);

        var user    = TherapistUser();
        var handler = new DeleteNoteCommandHandler(repo.Object, user.Object);

        var result = await handler.Handle(new DeleteNoteCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteHandler_TherapistNotOwner_ThrowsUnauthorizedAccessException()
    {
        var note = BuildDraftNote(therapistId: Guid.NewGuid());
        var repo = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);

        var user    = TherapistUser(TherapistId); // different therapist
        var handler = new DeleteNoteCommandHandler(repo.Object, user.Object);

        var act = () => handler.Handle(new DeleteNoteCommand(note.Id), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not the owner*");
    }

    [Fact]
    public async Task DeleteHandler_SignedNote_ThrowsInvalidOperationException()
    {
        // Domain guard: only Draft notes can be soft-deleted
        var note = BuildSignedNote(therapistId: TherapistId);
        var repo = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);

        var user    = TherapistUser(TherapistId);
        var handler = new DeleteNoteCommandHandler(repo.Object, user.Object);

        var act = () => handler.Handle(new DeleteNoteCommand(note.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*'{NoteStatus.Signed}'*");
    }

    // -----------------------------------------------------------------------
    // LockNoteCommandHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task LockHandler_SignedNote_LocksSuccessfully()
    {
        var note = BuildSignedNote(therapistId: TherapistId);
        var repo = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);

        var user    = TherapistUser(TherapistId);
        var handler = new LockNoteCommandHandler(repo.Object, user.Object);

        var dto = await handler.Handle(new LockNoteCommand(note.Id), CancellationToken.None);

        dto.Should().NotBeNull();
        dto!.Status.Should().Be(NoteStatus.Locked);
        dto.LockedByUserId.Should().Be(TherapistId);
        dto.LockedByFullName.Should().Be("Dr. Jane Smith");
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LockHandler_NoteNotFound_ReturnsNull()
    {
        var repo = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Note?)null);

        var user    = TherapistUser();
        var handler = new LockNoteCommandHandler(repo.Object, user.Object);

        var dto = await handler.Handle(new LockNoteCommand(Guid.NewGuid()), CancellationToken.None);

        dto.Should().BeNull();
    }

    [Fact]
    public async Task LockHandler_TherapistNotOwner_ThrowsUnauthorizedAccessException()
    {
        var note = BuildSignedNote(therapistId: Guid.NewGuid());
        var repo = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);

        var user    = TherapistUser(TherapistId);
        var handler = new LockNoteCommandHandler(repo.Object, user.Object);

        var act = () => handler.Handle(new LockNoteCommand(note.Id), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not the owner*");
    }

    [Fact]
    public async Task LockHandler_DraftNote_ThrowsInvalidOperationException()
    {
        var note = BuildDraftNote(therapistId: TherapistId);
        var repo = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);

        var user    = TherapistUser(TherapistId);
        var handler = new LockNoteCommandHandler(repo.Object, user.Object);

        var act = () => handler.Handle(new LockNoteCommand(note.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*'{NoteStatus.Draft}'*");
    }

    // -----------------------------------------------------------------------
    // SignNoteCommandHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task SignHandler_DraftNote_SignsSuccessfully()
    {
        var note = BuildDraftNote(therapistId: TherapistId);
        var repo = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);

        var user    = TherapistUser(TherapistId);
        var handler = new SignNoteCommandHandler(repo.Object, user.Object);

        var dto = await handler.Handle(new SignNoteCommand(note.Id), CancellationToken.None);

        dto.Should().NotBeNull();
        dto!.Status.Should().Be(NoteStatus.Signed);
        dto.SignedByUserId.Should().Be(TherapistId);
        dto.SignedByFullName.Should().Be("Dr. Jane Smith");
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SignHandler_NoteNotFound_ReturnsNull()
    {
        var repo = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Note?)null);

        var user    = TherapistUser();
        var handler = new SignNoteCommandHandler(repo.Object, user.Object);

        var dto = await handler.Handle(new SignNoteCommand(Guid.NewGuid()), CancellationToken.None);

        dto.Should().BeNull();
    }

    [Fact]
    public async Task SignHandler_TherapistNotOwner_ThrowsUnauthorizedAccessException()
    {
        var note = BuildDraftNote(therapistId: Guid.NewGuid());
        var repo = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);

        var user    = TherapistUser(TherapistId);
        var handler = new SignNoteCommandHandler(repo.Object, user.Object);

        var act = () => handler.Handle(new SignNoteCommand(note.Id), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not the owner*");
    }

    [Fact]
    public async Task SignHandler_AlreadySignedNote_ThrowsInvalidOperationException()
    {
        var note = BuildSignedNote(therapistId: TherapistId);
        var repo = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);

        var user    = TherapistUser(TherapistId);
        var handler = new SignNoteCommandHandler(repo.Object, user.Object);

        var act = () => handler.Handle(new SignNoteCommand(note.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*'{NoteStatus.Signed}'*");
    }

    // -----------------------------------------------------------------------
    // RequestCoSignCommandHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RequestCoSignHandler_DraftNote_SetsPendingCoSign()
    {
        var note = BuildDraftNote(therapistId: TherapistId);
        var repo = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);

        var user    = TherapistUser(TherapistId);
        var handler = new RequestCoSignCommandHandler(repo.Object, user.Object);

        var dto = await handler.Handle(new RequestCoSignCommand(note.Id), CancellationToken.None);

        dto.Should().NotBeNull();
        dto!.Status.Should().Be(NoteStatus.PendingCoSign);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RequestCoSignHandler_NoteNotFound_ReturnsNull()
    {
        var repo = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Note?)null);

        var user    = TherapistUser();
        var handler = new RequestCoSignCommandHandler(repo.Object, user.Object);

        var dto = await handler.Handle(new RequestCoSignCommand(Guid.NewGuid()), CancellationToken.None);

        dto.Should().BeNull();
    }

    [Fact]
    public async Task RequestCoSignHandler_TherapistNotOwner_ThrowsUnauthorizedAccessException()
    {
        var note = BuildDraftNote(therapistId: Guid.NewGuid());
        var repo = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);

        var user    = TherapistUser(TherapistId);
        var handler = new RequestCoSignCommandHandler(repo.Object, user.Object);

        var act = () => handler.Handle(new RequestCoSignCommand(note.Id), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not the owner*");
    }

    // -----------------------------------------------------------------------
    // AmendNoteCommandHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AmendHandler_LockedNote_CreatesNewDraftNote_AndMarksOriginalAmended()
    {
        var original = BuildLockedNote(therapistId: TherapistId);
        var repo     = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByIdAsync(original.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(original);
        repo.Setup(r => r.AddAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Note n, CancellationToken _) => n);

        var tenant  = ValidTenant();
        var user    = TherapistUser(TherapistId);
        var handler = new AmendNoteCommandHandler(repo.Object, tenant.Object, user.Object);
        var cmd     = new AmendNoteCommand(original.Id, "Amended title", "Amended content.", null);

        var dto = await handler.Handle(cmd, CancellationToken.None);

        // Returned DTO is the new draft
        dto.Should().NotBeNull();
        dto!.Status.Should().Be(NoteStatus.Draft);
        dto.AmendedFromId.Should().Be(original.Id);
        dto.Content.Should().Be("Amended content.");

        // Original note was marked as Amended
        original.Status.Should().Be(NoteStatus.Amended);

        repo.Verify(r => r.AddAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AmendHandler_NoTenantContext_ThrowsInvalidOperationException()
    {
        var repo    = new Mock<INoteRepository>();
        var tenant  = NoTenant();
        var user    = TherapistUser();
        var handler = new AmendNoteCommandHandler(repo.Object, tenant.Object, user.Object);
        var cmd     = new AmendNoteCommand(Guid.NewGuid(), null, "content", null);

        var act = () => handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Tenant context*");
    }

    [Fact]
    public async Task AmendHandler_NoteNotFound_ReturnsNull()
    {
        var repo = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Note?)null);

        var tenant  = ValidTenant();
        var user    = TherapistUser();
        var handler = new AmendNoteCommandHandler(repo.Object, tenant.Object, user.Object);
        var cmd     = new AmendNoteCommand(Guid.NewGuid(), null, "content", null);

        var dto = await handler.Handle(cmd, CancellationToken.None);

        dto.Should().BeNull();
    }

    [Fact]
    public async Task AmendHandler_DraftNote_ThrowsInvalidOperationException()
    {
        var note = BuildDraftNote(therapistId: TherapistId);
        var repo = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);

        var tenant  = ValidTenant();
        var user    = TherapistUser(TherapistId);
        var handler = new AmendNoteCommandHandler(repo.Object, tenant.Object, user.Object);
        var cmd     = new AmendNoteCommand(note.Id, null, "content", null);

        var act = () => handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*'{NoteStatus.Draft}'*");
    }

    [Fact]
    public async Task AmendHandler_TherapistNotOwner_ThrowsUnauthorizedAccessException()
    {
        var note = BuildLockedNote(therapistId: Guid.NewGuid());
        var repo = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);

        var tenant  = ValidTenant();
        var user    = TherapistUser(TherapistId);
        var handler = new AmendNoteCommandHandler(repo.Object, tenant.Object, user.Object);
        var cmd     = new AmendNoteCommand(note.Id, null, "content", null);

        var act = () => handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not the owner*");
    }
}
