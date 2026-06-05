using FluentAssertions;
using Sophrosync.Consent.Domain.Entities;
using Sophrosync.Consent.Domain.Enums;

namespace Sophrosync.Consent.Tests;

public sealed class ConsentEntityTests
{
    private static readonly Guid TenantId  = Guid.NewGuid();
    private static readonly Guid ClientId  = Guid.NewGuid();
    private static readonly Guid TemplateId = Guid.NewGuid();

    // -----------------------------------------------------------------------
    // ConsentTemplate
    // -----------------------------------------------------------------------

    [Fact]
    public void ConsentTemplate_Create_WithValidArgs_SetsDraftStatus()
    {
        var template = ConsentTemplate.Create(TenantId, ConsentPurpose.Treatment, "GDPR Consent", "I agree...");

        template.TenantId.Should().Be(TenantId);
        template.Purpose.Should().Be(ConsentPurpose.Treatment);
        template.Title.Should().Be("GDPR Consent");
        template.Status.Should().Be(ConsentTemplateStatus.Draft);
        template.VersionNumber.Should().Be(1);
        template.PublishedAt.Should().BeNull();
        template.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void ConsentTemplate_Create_WithEmptyTitle_ThrowsArgumentException()
    {
        var act = () => ConsentTemplate.Create(TenantId, ConsentPurpose.Treatment, "", "Body text.");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ConsentTemplate_Create_WithEmptyBodyText_ThrowsArgumentException()
    {
        var act = () => ConsentTemplate.Create(TenantId, ConsentPurpose.Treatment, "Title", "");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ConsentTemplate_Publish_DraftTemplate_SetsPublishedStatus()
    {
        var template = ConsentTemplate.Create(TenantId, ConsentPurpose.Treatment, "GDPR Consent", "Body.");

        template.Publish();

        template.Status.Should().Be(ConsentTemplateStatus.Published);
        template.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public void ConsentTemplate_Publish_AlreadyPublished_ThrowsInvalidOperationException()
    {
        var template = ConsentTemplate.Create(TenantId, ConsentPurpose.Treatment, "GDPR Consent", "Body.");
        template.Publish();

        var act = () => template.Publish();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ConsentTemplate_Retire_PublishedTemplate_SetsRetiredStatus()
    {
        var template = ConsentTemplate.Create(TenantId, ConsentPurpose.Treatment, "GDPR Consent", "Body.");
        template.Publish();

        template.Retire();

        template.Status.Should().Be(ConsentTemplateStatus.Retired);
        template.RetiredAt.Should().NotBeNull();
    }

    [Fact]
    public void ConsentTemplate_Retire_DraftTemplate_ThrowsInvalidOperationException()
    {
        var template = ConsentTemplate.Create(TenantId, ConsentPurpose.Treatment, "GDPR Consent", "Body.");

        var act = () => template.Retire();

        act.Should().Throw<InvalidOperationException>();
    }

    // -----------------------------------------------------------------------
    // ConsentRequest
    // -----------------------------------------------------------------------

    [Fact]
    public void ConsentRequest_Create_WithValidArgs_SetsPendingStatus()
    {
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var request   = ConsentRequest.Create(TenantId, ClientId, TemplateId, expiresAt);

        request.TenantId.Should().Be(TenantId);
        request.ClientId.Should().Be(ClientId);
        request.ConsentTemplateId.Should().Be(TemplateId);
        request.Status.Should().Be(ConsentRequestStatus.Pending);
        request.ExpiresAt.Should().Be(expiresAt);
        request.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void ConsentRequest_Create_WithPastExpiresAt_ThrowsArgumentException()
    {
        var act = () => ConsentRequest.Create(TenantId, ClientId, TemplateId, DateTime.UtcNow.AddDays(-1));

        act.Should().Throw<ArgumentException>().WithMessage("*ExpiresAt*");
    }

    [Fact]
    public void ConsentRequest_Complete_PendingRequest_SetsCompletedStatus()
    {
        var request = ConsentRequest.Create(TenantId, ClientId, TemplateId, DateTime.UtcNow.AddDays(7));

        request.Complete();

        request.Status.Should().Be(ConsentRequestStatus.Completed);
        request.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void ConsentRequest_Complete_AlreadyCompleted_ThrowsInvalidOperationException()
    {
        var request = ConsentRequest.Create(TenantId, ClientId, TemplateId, DateTime.UtcNow.AddDays(7));
        request.Complete();

        var act = () => request.Complete();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ConsentRequest_Revoke_SetsRevokedStatus()
    {
        var request = ConsentRequest.Create(TenantId, ClientId, TemplateId, DateTime.UtcNow.AddDays(7));

        request.Revoke();

        request.Status.Should().Be(ConsentRequestStatus.Revoked);
        request.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public void ConsentRequest_Expire_PendingRequest_SetsExpiredStatus()
    {
        var request = ConsentRequest.Create(TenantId, ClientId, TemplateId, DateTime.UtcNow.AddDays(7));

        request.Expire();

        request.Status.Should().Be(ConsentRequestStatus.Expired);
    }

    [Fact]
    public void ConsentRequest_Expire_AlreadyCompleted_NoStateChange()
    {
        var request = ConsentRequest.Create(TenantId, ClientId, TemplateId, DateTime.UtcNow.AddDays(7));
        request.Complete();

        request.Expire();

        request.Status.Should().Be(ConsentRequestStatus.Completed);
    }
}
