using FluentAssertions;
using FluentValidation.TestHelper;
using Sophrosync.Notifications.Application.Commands.SendNotification;
using Sophrosync.Notifications.Application.Validators;
using Sophrosync.Notifications.Domain.Enums;

namespace Sophrosync.Notifications.Tests;

public sealed class ValidatorTests
{
    private readonly SendNotificationCommandValidator _validator = new();

    private static SendNotificationCommand ValidCommand() => new(
        TenantId: Guid.NewGuid(),
        RecipientUserId: Guid.NewGuid(),
        Channel: NotificationChannel.InApp,
        Type: NotificationType.General,
        Subject: "Valid subject",
        Body: "Valid body",
        ScheduledFor: DateTime.UtcNow,
        CorrelationId: "corr-valid");

    [Fact]
    public void ValidCommand_PassesValidation()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyTenantId_FailsValidation()
    {
        var cmd = ValidCommand() with { TenantId = Guid.Empty };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Fact]
    public void EmptyRecipientUserId_FailsValidation()
    {
        var cmd = ValidCommand() with { RecipientUserId = Guid.Empty };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.RecipientUserId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptySubject_FailsValidation(string subject)
    {
        var cmd = ValidCommand() with { Subject = subject };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Subject);
    }

    [Fact]
    public void SubjectExceeds500Chars_FailsValidation()
    {
        var cmd = ValidCommand() with { Subject = new string('x', 501) };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Subject);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyBody_FailsValidation(string body)
    {
        var cmd = ValidCommand() with { Body = body };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Body);
    }

    [Fact]
    public void BodyExceeds10000Chars_FailsValidation()
    {
        var cmd = ValidCommand() with { Body = new string('x', 10001) };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Body);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyCorrelationId_FailsValidation(string correlationId)
    {
        var cmd = ValidCommand() with { CorrelationId = correlationId };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.CorrelationId);
    }

    [Fact]
    public void CorrelationIdExceeds200Chars_FailsValidation()
    {
        var cmd = ValidCommand() with { CorrelationId = new string('x', 201) };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.CorrelationId);
    }

    [Fact]
    public void ScheduledForMoreThan5MinInPast_FailsValidation()
    {
        var cmd = ValidCommand() with { ScheduledFor = DateTime.UtcNow.AddMinutes(-6) };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.ScheduledFor);
    }

    [Fact]
    public void ScheduledForFuture_PassesValidation()
    {
        var cmd = ValidCommand() with { ScheduledFor = DateTime.UtcNow.AddHours(1) };
        _validator.TestValidate(cmd).ShouldNotHaveValidationErrorFor(x => x.ScheduledFor);
    }
}
