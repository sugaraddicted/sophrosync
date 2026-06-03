using FluentAssertions;
using FluentValidation.TestHelper;
using Sophrosync.Schedule.Application.Commands.CancelAppointment;
using Sophrosync.Schedule.Application.Commands.RescheduleAppointment;
using Sophrosync.Schedule.Application.Commands.ScheduleAppointment;

namespace Sophrosync.Schedule.Tests.Application;

public sealed class ValidatorTests
{
    // -----------------------------------------------------------------------
    // ScheduleAppointmentCommandValidator
    // -----------------------------------------------------------------------

    private readonly ScheduleAppointmentCommandValidator _scheduleValidator = new();

    private static ScheduleAppointmentCommand ValidScheduleCommand() => new(
        Guid.NewGuid(),
        Guid.NewGuid(),
        DateTime.UtcNow.AddDays(1),
        60,
        "InPerson",
        null);

    [Fact]
    public void ScheduleValidator_ValidCommand_Passes()
    {
        var result = _scheduleValidator.TestValidate(ValidScheduleCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ScheduleValidator_EmptyClientId_Fails()
    {
        var cmd = ValidScheduleCommand() with { ClientId = Guid.Empty };
        var result = _scheduleValidator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.ClientId);
    }

    [Fact]
    public void ScheduleValidator_EmptyTherapistId_Fails()
    {
        var cmd = ValidScheduleCommand() with { TherapistId = Guid.Empty };
        var result = _scheduleValidator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.TherapistId);
    }

    [Fact]
    public void ScheduleValidator_PastDate_Fails()
    {
        var cmd = ValidScheduleCommand() with { ScheduledAt = DateTime.UtcNow.AddHours(-1) };
        var result = _scheduleValidator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.ScheduledAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ScheduleValidator_InvalidDuration_Fails(int duration)
    {
        var cmd = ValidScheduleCommand() with { DurationMinutes = duration };
        var result = _scheduleValidator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes);
    }

    [Fact]
    public void ScheduleValidator_DurationExceeds480_Fails()
    {
        var cmd = ValidScheduleCommand() with { DurationMinutes = 481 };
        var result = _scheduleValidator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("UnknownType")]
    [InlineData("Appointment")]
    public void ScheduleValidator_InvalidType_Fails(string type)
    {
        var cmd = ValidScheduleCommand() with { Type = type };
        var result = _scheduleValidator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    [Theory]
    [InlineData("InPerson")]
    [InlineData("Video")]
    [InlineData("Phone")]
    public void ScheduleValidator_ValidTypes_Pass(string type)
    {
        var cmd = ValidScheduleCommand() with { Type = type };
        var result = _scheduleValidator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.Type);
    }

    [Fact]
    public void ScheduleValidator_NotesTooLong_Fails()
    {
        var cmd = ValidScheduleCommand() with { Notes = new string('x', 5001) };
        var result = _scheduleValidator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }

    // -----------------------------------------------------------------------
    // CancelAppointmentCommandValidator
    // -----------------------------------------------------------------------

    private readonly CancelAppointmentCommandValidator _cancelValidator = new();

    [Fact]
    public void CancelValidator_ValidCommand_Passes()
    {
        var result = _cancelValidator.TestValidate(
            new CancelAppointmentCommand(Guid.NewGuid(), "client request"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CancelValidator_EmptyId_Fails()
    {
        var result = _cancelValidator.TestValidate(
            new CancelAppointmentCommand(Guid.Empty, "reason"));
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CancelValidator_EmptyReason_Fails(string reason)
    {
        var result = _cancelValidator.TestValidate(
            new CancelAppointmentCommand(Guid.NewGuid(), reason));
        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void CancelValidator_ReasonTooLong_Fails()
    {
        var result = _cancelValidator.TestValidate(
            new CancelAppointmentCommand(Guid.NewGuid(), new string('x', 501)));
        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }

    // -----------------------------------------------------------------------
    // RescheduleAppointmentCommandValidator
    // -----------------------------------------------------------------------

    private readonly RescheduleAppointmentCommandValidator _rescheduleValidator = new();

    [Fact]
    public void RescheduleValidator_ValidCommand_Passes()
    {
        var result = _rescheduleValidator.TestValidate(
            new RescheduleAppointmentCommand(Guid.NewGuid(), DateTime.UtcNow.AddDays(3), null));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RescheduleValidator_PastDate_Fails()
    {
        var result = _rescheduleValidator.TestValidate(
            new RescheduleAppointmentCommand(Guid.NewGuid(), DateTime.UtcNow.AddHours(-1), null));
        result.ShouldHaveValidationErrorFor(x => x.NewScheduledAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void RescheduleValidator_InvalidDuration_Fails(int duration)
    {
        var result = _rescheduleValidator.TestValidate(
            new RescheduleAppointmentCommand(Guid.NewGuid(), DateTime.UtcNow.AddDays(1), duration));
        result.ShouldHaveValidationErrorFor(x => x.NewDurationMinutes);
    }

    [Fact]
    public void RescheduleValidator_DurationOver480_Fails()
    {
        var result = _rescheduleValidator.TestValidate(
            new RescheduleAppointmentCommand(Guid.NewGuid(), DateTime.UtcNow.AddDays(1), 481));
        result.ShouldHaveValidationErrorFor(x => x.NewDurationMinutes);
    }

    [Fact]
    public void RescheduleValidator_NullDuration_Passes()
    {
        var result = _rescheduleValidator.TestValidate(
            new RescheduleAppointmentCommand(Guid.NewGuid(), DateTime.UtcNow.AddDays(1), null));
        result.ShouldNotHaveValidationErrorFor(x => x.NewDurationMinutes);
    }
}
