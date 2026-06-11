using FluentAssertions;
using MediatR;
using Moq;
using Sophrosync.Notifications.Application.Commands.DismissNotification;
using Sophrosync.Notifications.Application.Commands.SendNotification;
using Sophrosync.Notifications.Domain.Entities;
using Sophrosync.Notifications.Domain.Enums;
using Sophrosync.Notifications.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Notifications.Tests;

public sealed class CommandHandlerTests
{
    private readonly Mock<INotificationRepository> _repo = new();
    private readonly Mock<INotificationPreferenceRepository> _prefs = new();
    private readonly Mock<IPublisher> _publisher = new();

    public CommandHandlerTests()
    {
        _publisher
            .Setup(p => p.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private SendNotificationCommand BuildCommand(string correlationId = "corr-1") =>
        new(
            TenantId: Guid.NewGuid(),
            RecipientUserId: Guid.NewGuid(),
            Channel: NotificationChannel.InApp,
            Type: NotificationType.General,
            Subject: "Hello",
            Body: "World",
            ScheduledFor: DateTime.UtcNow,
            CorrelationId: correlationId);

    private SendNotificationCommandHandler BuildSendHandler() =>
        new(_repo.Object, _prefs.Object, _publisher.Object);

    // ── SendNotificationCommandHandler ──────────────────────────────────────

    [Fact]
    public async Task SendHandler_NoPrefsExist_CreatesInAppOnly()
    {
        _repo.Setup(r => r.GetByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification?)null);
        _prefs.Setup(p => p.GetForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationPreference?)null);

        var addedNotifications = new List<Notification>();
        _repo.Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((n, _) => addedNotifications.Add(n));

        var cmd = BuildCommand();
        var result = await BuildSendHandler().Handle(cmd, default);

        result.Should().NotBe(Guid.Empty);
        addedNotifications.Should().ContainSingle(n => n.Channel == NotificationChannel.InApp);
    }

    [Fact]
    public async Task SendHandler_ExistingCorrelationId_ReturnsExistingIdWithoutCreating()
    {
        var existing = Notification.Create(Guid.NewGuid(), Guid.NewGuid(),
            NotificationChannel.InApp, NotificationType.General,
            "s", "b", DateTime.UtcNow, "corr-existing");
        existing.ClearDomainEvents();

        _repo.Setup(r => r.GetByCorrelationIdAsync("corr-existing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var cmd = BuildCommand("corr-existing");
        var result = await BuildSendHandler().Handle(cmd, default);

        result.Should().Be(existing.Id);
        _repo.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendHandler_EmailAndInAppEnabled_CreatesBothChannels()
    {
        _repo.Setup(r => r.GetByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification?)null);

        var prefs = NotificationPreference.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            NotificationChannel.Email,
            emailEnabled: true, inAppEnabled: true, smsEnabled: false,
            emailAddress: "user@example.com");

        _prefs.Setup(p => p.GetForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(prefs);

        var addedNotifications = new List<Notification>();
        _repo.Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((n, _) => addedNotifications.Add(n));

        await BuildSendHandler().Handle(BuildCommand(), default);

        addedNotifications.Should().HaveCount(2);
        addedNotifications.Should().Contain(n => n.Channel == NotificationChannel.InApp);
        addedNotifications.Should().Contain(n => n.Channel == NotificationChannel.Email);
    }

    [Fact]
    public async Task SendHandler_EmailEnabledButNoAddress_CreatesInAppOnly()
    {
        _repo.Setup(r => r.GetByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification?)null);

        var prefs = NotificationPreference.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            NotificationChannel.Email,
            emailEnabled: true, inAppEnabled: true, smsEnabled: false,
            emailAddress: null);

        _prefs.Setup(p => p.GetForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(prefs);

        var addedNotifications = new List<Notification>();
        _repo.Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((n, _) => addedNotifications.Add(n));

        await BuildSendHandler().Handle(BuildCommand(), default);

        addedNotifications.Should().ContainSingle(n => n.Channel == NotificationChannel.InApp);
    }

    [Fact]
    public async Task SendHandler_InAppDisabledEmailEnabled_CreatesEmailOnly()
    {
        _repo.Setup(r => r.GetByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification?)null);

        var prefs = NotificationPreference.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            NotificationChannel.Email,
            emailEnabled: true, inAppEnabled: false, smsEnabled: false,
            emailAddress: "user@example.com");

        _prefs.Setup(p => p.GetForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(prefs);

        var addedNotifications = new List<Notification>();
        _repo.Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((n, _) => addedNotifications.Add(n));

        var result = await BuildSendHandler().Handle(BuildCommand(), default);

        result.Should().NotBe(Guid.Empty);
        addedNotifications.Should().ContainSingle(n => n.Channel == NotificationChannel.Email);
    }

    [Fact]
    public async Task SendHandler_EmailFanOut_UsesCorrelationIdWithSuffix()
    {
        _repo.Setup(r => r.GetByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification?)null);

        var prefs = NotificationPreference.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            NotificationChannel.Email,
            emailEnabled: true, inAppEnabled: true, smsEnabled: false,
            emailAddress: "user@example.com");

        _prefs.Setup(p => p.GetForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(prefs);

        var addedNotifications = new List<Notification>();
        _repo.Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((n, _) => addedNotifications.Add(n));

        await BuildSendHandler().Handle(BuildCommand("my-corr"), default);

        addedNotifications.Should().Contain(n => n.CorrelationId == "my-corr");
        addedNotifications.Should().Contain(n => n.CorrelationId == "my-corr:email");
    }

    // ── DismissNotificationCommandHandler ───────────────────────────────────

    [Fact]
    public async Task DismissHandler_ValidNotification_SetsDismissedStatus()
    {
        var notification = Notification.Create(
            Guid.NewGuid(), Guid.NewGuid(), NotificationChannel.InApp,
            NotificationType.General, "s", "b", DateTime.UtcNow, "corr-d");
        notification.ClearDomainEvents();

        _repo.Setup(r => r.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        var handler = new DismissNotificationCommandHandler(_repo.Object);
        await handler.Handle(new DismissNotificationCommand(notification.Id), default);

        notification.Status.Should().Be(NotificationStatus.Dismissed);
        _repo.Verify(r => r.Update(notification), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DismissHandler_NotFound_ThrowsInvalidOperationException()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification?)null);

        var handler = new DismissNotificationCommandHandler(_repo.Object);
        var act = () => handler.Handle(new DismissNotificationCommand(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
