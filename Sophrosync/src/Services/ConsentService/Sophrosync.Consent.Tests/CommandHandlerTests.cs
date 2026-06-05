using FluentAssertions;
using MediatR;
using Moq;
using Sophrosync.Consent.Application.Commands.CreateConsentTemplate;
using Sophrosync.Consent.Application.Commands.IssueConsentRequest;
using Sophrosync.Consent.Application.Commands.PublishConsentTemplate;
using Sophrosync.Consent.Application.Commands.RetireConsentTemplate;
using Sophrosync.Consent.Application.Commands.RevokeConsentRequest;
using Sophrosync.Consent.Domain.Entities;
using Sophrosync.Consent.Domain.Enums;
using Sophrosync.Consent.Domain.Interfaces;

namespace Sophrosync.Consent.Tests;

public sealed class CommandHandlerTests
{
    private static readonly Guid TenantId  = Guid.NewGuid();
    private static readonly Guid ClientId  = Guid.NewGuid();

    private static ConsentTemplate BuildDraftTemplate() =>
        ConsentTemplate.Create(TenantId, ConsentPurpose.Treatment, "GDPR Consent", "I agree to treatment.");

    private static ConsentTemplate BuildPublishedTemplate()
    {
        var t = BuildDraftTemplate();
        t.Publish();
        return t;
    }

    // -----------------------------------------------------------------------
    // CreateConsentTemplateCommandHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateTemplate_ValidCommand_CallsAddAndSave_ReturnsId()
    {
        var repo = new Mock<IConsentTemplateRepository>();
        var handler = new CreateConsentTemplateCommandHandler(repo.Object);
        var cmd = new CreateConsentTemplateCommand(TenantId, ConsentPurpose.Treatment, "GDPR Consent", "Body text.");

        var id = await handler.Handle(cmd, CancellationToken.None);

        id.Should().NotBeEmpty();
        repo.Verify(r => r.AddAsync(It.IsAny<ConsentTemplate>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // -----------------------------------------------------------------------
    // PublishConsentTemplateCommandHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task PublishTemplate_DraftTemplate_PublishesSuccessfully()
    {
        var template  = BuildDraftTemplate();
        var repo      = new Mock<IConsentTemplateRepository>();
        var publisher = new Mock<IPublisher>();
        repo.Setup(r => r.GetByIdAsync(template.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var handler = new PublishConsentTemplateCommandHandler(repo.Object, publisher.Object);
        await handler.Handle(new PublishConsentTemplateCommand(template.Id), CancellationToken.None);

        template.Status.Should().Be(ConsentTemplateStatus.Published);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishTemplate_NonExistentId_ThrowsInvalidOperationException()
    {
        var repo      = new Mock<IConsentTemplateRepository>();
        var publisher = new Mock<IPublisher>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsentTemplate?)null);

        var handler = new PublishConsentTemplateCommandHandler(repo.Object, publisher.Object);
        var act     = () => handler.Handle(new PublishConsentTemplateCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // -----------------------------------------------------------------------
    // RetireConsentTemplateCommandHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RetireTemplate_PublishedTemplate_RetiresSuccessfully()
    {
        var template = BuildPublishedTemplate();
        var repo     = new Mock<IConsentTemplateRepository>();
        repo.Setup(r => r.GetByIdAsync(template.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var handler = new RetireConsentTemplateCommandHandler(repo.Object);
        await handler.Handle(new RetireConsentTemplateCommand(template.Id), CancellationToken.None);

        template.Status.Should().Be(ConsentTemplateStatus.Retired);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // -----------------------------------------------------------------------
    // IssueConsentRequestCommandHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task IssueRequest_ValidCommand_CallsAddAndSave_ReturnsId()
    {
        var repo    = new Mock<IConsentRequestRepository>();
        var handler = new IssueConsentRequestCommandHandler(repo.Object);
        var cmd     = new IssueConsentRequestCommand(TenantId, ClientId, Guid.NewGuid(), DateTime.UtcNow.AddDays(7));

        var id = await handler.Handle(cmd, CancellationToken.None);

        id.Should().NotBeEmpty();
        repo.Verify(r => r.AddAsync(It.IsAny<ConsentRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // -----------------------------------------------------------------------
    // RevokeConsentRequestCommandHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RevokeRequest_ExistingRequest_RevokesSuccessfully()
    {
        var request = ConsentRequest.Create(TenantId, ClientId, Guid.NewGuid(), DateTime.UtcNow.AddDays(7));
        var repo    = new Mock<IConsentRequestRepository>();
        repo.Setup(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        var handler = new RevokeConsentRequestCommandHandler(repo.Object);
        await handler.Handle(new RevokeConsentRequestCommand(request.Id), CancellationToken.None);

        request.Status.Should().Be(ConsentRequestStatus.Revoked);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeRequest_NonExistentId_ThrowsInvalidOperationException()
    {
        var repo = new Mock<IConsentRequestRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsentRequest?)null);

        var handler = new RevokeConsentRequestCommandHandler(repo.Object);
        var act     = () => handler.Handle(new RevokeConsentRequestCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
