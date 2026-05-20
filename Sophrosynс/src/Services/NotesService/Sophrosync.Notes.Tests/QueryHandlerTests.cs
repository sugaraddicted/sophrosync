using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Sophrosync.Notes.Application.Queries.GetNoteById;
using Sophrosync.Notes.Application.Queries.GetNotes;
using Sophrosync.Notes.Application.Queries.GetNotesByClientId;
using Sophrosync.Notes.Domain.Entities;
using Sophrosync.Notes.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Notes.Tests;

/// <summary>
/// Unit tests for query handlers. All I/O dependencies are mocked.
/// </summary>
public sealed class QueryHandlerTests
{
    // -----------------------------------------------------------------------
    // Shared helpers
    // -----------------------------------------------------------------------

    private static readonly Guid TenantId    = Guid.NewGuid();
    private static readonly Guid TherapistId = Guid.NewGuid();
    private static readonly Guid ClientId    = Guid.NewGuid();

    private static Mock<ICurrentTenant> Tenant()
    {
        var mock = new Mock<ICurrentTenant>();
        mock.Setup(t => t.Id).Returns(TenantId);
        mock.Setup(t => t.HasTenant).Returns(true);
        return mock;
    }

    private static Mock<ICurrentUser> User()
    {
        var mock = new Mock<ICurrentUser>();
        mock.Setup(u => u.Id).Returns(TherapistId);
        mock.Setup(u => u.FullName).Returns("Dr. Jane Smith");
        return mock;
    }

    private static Note BuildNote(Guid? clientId = null) =>
        Note.Create(
            TenantId,
            clientId ?? ClientId,
            appointmentId: null,
            TherapistId,
            "Dr. Jane Smith",
            NoteType.DAP,
            "Title",
            "Content",
            tags: null);

    // -----------------------------------------------------------------------
    // GetNoteByIdQueryHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetNoteByIdHandler_ExistingNote_ReturnsCorrectDto()
    {
        var note = BuildNote();
        var repo = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);

        var handler = new GetNoteByIdQueryHandler(
            repo.Object,
            Tenant().Object,
            User().Object,
            NullLogger<GetNoteByIdQueryHandler>.Instance);

        var dto = await handler.Handle(new GetNoteByIdQuery(note.Id), CancellationToken.None);

        dto.Should().NotBeNull();
        dto!.Id.Should().Be(note.Id);
        dto.ClientId.Should().Be(note.ClientId);
        dto.Status.Should().Be(NoteStatus.Draft);
        dto.TherapistId.Should().Be(TherapistId);
    }

    [Fact]
    public async Task GetNoteByIdHandler_NoteNotFound_ReturnsNull()
    {
        var repo = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Note?)null);

        var handler = new GetNoteByIdQueryHandler(
            repo.Object,
            Tenant().Object,
            User().Object,
            NullLogger<GetNoteByIdQueryHandler>.Instance);

        var dto = await handler.Handle(new GetNoteByIdQuery(Guid.NewGuid()), CancellationToken.None);

        dto.Should().BeNull();
    }

    // -----------------------------------------------------------------------
    // GetNotesQueryHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetNotesHandler_ReturnsAllNotesForTenant()
    {
        var notes = new List<Note> { BuildNote(), BuildNote(), BuildNote() };
        var repo  = new Mock<INoteRepository>();
        repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(notes);

        var handler = new GetNotesQueryHandler(
            repo.Object,
            Tenant().Object,
            User().Object,
            NullLogger<GetNotesQueryHandler>.Instance);

        var result = await handler.Handle(new GetNotesQuery(), CancellationToken.None);

        result.Should().HaveCount(3);
        result.Select(d => d.Id).Should().BeEquivalentTo(notes.Select(n => n.Id));
    }

    [Fact]
    public async Task GetNotesHandler_EmptyRepository_ReturnsEmptyList()
    {
        var repo = new Mock<INoteRepository>();
        repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Note>());

        var handler = new GetNotesQueryHandler(
            repo.Object,
            Tenant().Object,
            User().Object,
            NullLogger<GetNotesQueryHandler>.Instance);

        var result = await handler.Handle(new GetNotesQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    // -----------------------------------------------------------------------
    // GetNotesByClientIdQueryHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetNotesByClientIdHandler_ExistingClientNotes_ReturnsCorrectList()
    {
        var targetClient = Guid.NewGuid();
        var notes = new List<Note> { BuildNote(targetClient), BuildNote(targetClient) };
        var repo  = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByClientIdAsync(targetClient, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notes);

        var handler = new GetNotesByClientIdQueryHandler(
            repo.Object,
            Tenant().Object,
            User().Object,
            NullLogger<GetNotesByClientIdQueryHandler>.Instance);

        var result = await handler.Handle(
            new GetNotesByClientIdQuery(targetClient), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(d => d.ClientId.Should().Be(targetClient));
    }

    [Fact]
    public async Task GetNotesByClientIdHandler_NoNotesForClient_ReturnsEmptyList()
    {
        var repo = new Mock<INoteRepository>();
        repo.Setup(r => r.GetByClientIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Note>());

        var handler = new GetNotesByClientIdQueryHandler(
            repo.Object,
            Tenant().Object,
            User().Object,
            NullLogger<GetNotesByClientIdQueryHandler>.Instance);

        var result = await handler.Handle(
            new GetNotesByClientIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
