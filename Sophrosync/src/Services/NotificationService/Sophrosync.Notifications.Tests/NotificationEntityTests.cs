using FluentAssertions;
using Sophrosync.Notifications.Domain.Entities;
using Sophrosync.Notifications.Domain.Enums;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Notifications.Tests;

public sealed class NotificationEntityTests
{
    private static Notification BuildNotification(
        NotificationChannel channel = NotificationChannel.InApp,
        string correlationId = "corr-1") =>
        Notification.Create(
            tenantId: Guid.NewGuid(),
            recipientUserId: Guid.NewGuid(),
            channel: channel,
            type: NotificationType.General,
            subject: "Test subject",
            body: "Test body",
            scheduledFor: DateTime.UtcNow,
            correlationId: correlationId);

    [Fact]
    public void Create_ValidArgs_ReturnsPendingNotification()
    {
        var n = BuildNotification();

        n.Status.Should().Be(NotificationStatus.Pending);
        n.RetryCount.Should().Be(0);
        n.SentAt.Should().BeNull();
        n.DismissedAt.Should().BeNull();
        n.DeletedAt.Should().BeNull();
        n.Subject.Should().Be("Test subject");
        n.Body.Should().Be("Test body");
    }

    [Fact]
    public void Create_RaisesDomainEvent()
    {
        var n = BuildNotification();

        n.DomainEvents.Should().ContainSingle()
            .Which.Should().BeAssignableTo<IDomainEvent>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptySubject_Throws(string subject)
    {
        var act = () => Notification.Create(
            Guid.NewGuid(), Guid.NewGuid(), NotificationChannel.InApp,
            NotificationType.General, subject, "body", DateTime.UtcNow, "corr");

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyBody_Throws(string body)
    {
        var act = () => Notification.Create(
            Guid.NewGuid(), Guid.NewGuid(), NotificationChannel.InApp,
            NotificationType.General, "subject", body, DateTime.UtcNow, "corr");

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyCorrelationId_Throws(string correlationId)
    {
        var act = () => Notification.Create(
            Guid.NewGuid(), Guid.NewGuid(), NotificationChannel.InApp,
            NotificationType.General, "subject", "body", DateTime.UtcNow, correlationId);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MarkSent_SetsSentStatusAndTimestamp()
    {
        var before = DateTime.UtcNow;
        var n = BuildNotification();

        n.MarkSent();

        n.Status.Should().Be(NotificationStatus.Sent);
        n.SentAt.Should().NotBeNull().And.BeOnOrAfter(before);
    }

    [Fact]
    public void MarkSent_RaisesSentDomainEvent()
    {
        var n = BuildNotification();
        n.ClearDomainEvents();

        n.MarkSent();

        n.DomainEvents.Should().ContainSingle();
    }

    [Fact]
    public void MarkFailed_SetsFailedStatusAndReason()
    {
        var n = BuildNotification();

        n.MarkFailed("SMTP timeout");

        n.Status.Should().Be(NotificationStatus.Failed);
        n.FailureReason.Should().Be("SMTP timeout");
    }

    [Fact]
    public void IncrementRetry_IncrementsCountAndSetsRetryingStatus()
    {
        var n = BuildNotification();

        n.IncrementRetry();
        n.IncrementRetry();

        n.RetryCount.Should().Be(2);
        n.Status.Should().Be(NotificationStatus.Retrying);
    }

    [Fact]
    public void Dismiss_SetsDismissedStatusAndTimestamp()
    {
        var before = DateTime.UtcNow;
        var n = BuildNotification();

        n.Dismiss();

        n.Status.Should().Be(NotificationStatus.Dismissed);
        n.DismissedAt.Should().NotBeNull().And.BeOnOrAfter(before);
    }

    [Fact]
    public void SoftDelete_SetsDeletedAt()
    {
        var before = DateTime.UtcNow;
        var n = BuildNotification();

        n.SoftDelete();

        n.DeletedAt.Should().NotBeNull().And.BeOnOrAfter(before);
    }
}
