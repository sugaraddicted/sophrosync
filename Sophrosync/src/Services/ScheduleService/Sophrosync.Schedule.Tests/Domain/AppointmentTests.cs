using FluentAssertions;
using Sophrosync.Schedule.Domain.Entities;
using Sophrosync.Schedule.Domain.Enums;

namespace Sophrosync.Schedule.Tests.Domain;

public sealed class AppointmentTests
{
    private static Appointment MakeScheduled(int durationMinutes = 60) =>
        Appointment.Schedule(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1),
            durationMinutes,
            AppointmentType.InPerson);

    // -----------------------------------------------------------------------
    // Schedule factory
    // -----------------------------------------------------------------------

    [Fact]
    public void Schedule_ValidArgs_CreatesWithScheduledStatus()
    {
        var tenantId    = Guid.NewGuid();
        var clientId    = Guid.NewGuid();
        var therapistId = Guid.NewGuid();
        var at          = DateTime.UtcNow.AddHours(2);

        var appt = Appointment.Schedule(tenantId, clientId, therapistId, at, 50, AppointmentType.Video, "notes");

        appt.TenantId.Should().Be(tenantId);
        appt.ClientId.Should().Be(clientId);
        appt.TherapistId.Should().Be(therapistId);
        appt.ScheduledAt.Should().Be(at);
        appt.DurationMinutes.Should().Be(50);
        appt.Type.Should().Be(AppointmentType.Video);
        appt.Status.Should().Be(AppointmentStatus.Scheduled);
        appt.Notes.Should().Be("notes");
        appt.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-60)]
    public void Schedule_InvalidDuration_Throws(int duration)
    {
        var act = () => Appointment.Schedule(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1), duration, AppointmentType.InPerson);

        act.Should().Throw<ArgumentException>().WithMessage("*Duration*");
    }

    [Fact]
    public void Schedule_RaisesDomainEvent()
    {
        var appt = MakeScheduled();
        appt.DomainEvents.Should().ContainSingle(e => e.GetType().Name == "AppointmentScheduledDomainEvent");
    }

    // -----------------------------------------------------------------------
    // Confirm
    // -----------------------------------------------------------------------

    [Fact]
    public void Confirm_WhenScheduled_TransitionsToConfirmed()
    {
        var appt = MakeScheduled();
        appt.Confirm();
        appt.Status.Should().Be(AppointmentStatus.Confirmed);
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_Throws()
    {
        var appt = MakeScheduled();
        appt.Confirm();
        var act = () => appt.Confirm();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Confirm_WhenCompleted_Throws()
    {
        var appt = MakeScheduled();
        appt.Complete();
        var act = () => appt.Confirm();
        act.Should().Throw<InvalidOperationException>();
    }

    // -----------------------------------------------------------------------
    // Complete
    // -----------------------------------------------------------------------

    [Fact]
    public void Complete_WhenScheduled_TransitionsToCompleted()
    {
        var appt = MakeScheduled();
        appt.Complete("session notes");
        appt.Status.Should().Be(AppointmentStatus.Completed);
        appt.Notes.Should().Be("session notes");
    }

    [Fact]
    public void Complete_WhenConfirmed_TransitionsToCompleted()
    {
        var appt = MakeScheduled();
        appt.Confirm();
        appt.Complete();
        appt.Status.Should().Be(AppointmentStatus.Completed);
    }

    [Fact]
    public void Complete_WhenCancelled_Throws()
    {
        var appt = MakeScheduled();
        appt.Cancel("reason");
        var act = () => appt.Complete();
        act.Should().Throw<InvalidOperationException>();
    }

    // -----------------------------------------------------------------------
    // Cancel
    // -----------------------------------------------------------------------

    [Fact]
    public void Cancel_WhenScheduled_TransitionsToCancelled()
    {
        var appt = MakeScheduled();
        appt.Cancel("client request");
        appt.Status.Should().Be(AppointmentStatus.Cancelled);
        appt.CancellationReason.Should().Be("client request");
    }

    [Fact]
    public void Cancel_WhenCompleted_Throws()
    {
        var appt = MakeScheduled();
        appt.Complete();
        var act = () => appt.Cancel("reason");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_Throws()
    {
        var appt = MakeScheduled();
        appt.Cancel("first");
        var act = () => appt.Cancel("second");
        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Cancel_EmptyReason_Throws(string reason)
    {
        var appt = MakeScheduled();
        var act = () => appt.Cancel(reason);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Cancel_RaisesDomainEvent()
    {
        var appt = MakeScheduled();
        appt.ClearDomainEvents();
        appt.Cancel("no show");
        appt.DomainEvents.Should().ContainSingle(e => e.GetType().Name == "AppointmentCancelledDomainEvent");
    }

    // -----------------------------------------------------------------------
    // MarkNoShow
    // -----------------------------------------------------------------------

    [Fact]
    public void MarkNoShow_WhenScheduled_TransitionsToNoShow()
    {
        var appt = MakeScheduled();
        appt.MarkNoShow();
        appt.Status.Should().Be(AppointmentStatus.NoShow);
    }

    [Fact]
    public void MarkNoShow_WhenCompleted_Throws()
    {
        var appt = MakeScheduled();
        appt.Complete();
        var act = () => appt.MarkNoShow();
        act.Should().Throw<InvalidOperationException>();
    }

    // -----------------------------------------------------------------------
    // Reschedule
    // -----------------------------------------------------------------------

    [Fact]
    public void Reschedule_UpdatesTimeAndResetsToScheduled()
    {
        var appt = MakeScheduled();
        appt.Confirm();
        var newTime = DateTime.UtcNow.AddDays(7);

        appt.Reschedule(newTime, 90);

        appt.ScheduledAt.Should().Be(newTime);
        appt.DurationMinutes.Should().Be(90);
        appt.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    [Fact]
    public void Reschedule_WithoutDuration_KeepsExistingDuration()
    {
        var appt = MakeScheduled(45);
        appt.Reschedule(DateTime.UtcNow.AddDays(3));
        appt.DurationMinutes.Should().Be(45);
    }

    [Fact]
    public void Reschedule_WhenCompleted_Throws()
    {
        var appt = MakeScheduled();
        appt.Complete();
        var act = () => appt.Reschedule(DateTime.UtcNow.AddDays(1));
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reschedule_WhenCancelled_Throws()
    {
        var appt = MakeScheduled();
        appt.Cancel("reason");
        var act = () => appt.Reschedule(DateTime.UtcNow.AddDays(1));
        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Reschedule_InvalidDuration_Throws(int duration)
    {
        var appt = MakeScheduled();
        var act = () => appt.Reschedule(DateTime.UtcNow.AddDays(1), duration);
        act.Should().Throw<ArgumentException>();
    }
}
