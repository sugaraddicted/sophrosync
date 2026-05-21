using FluentAssertions;
using Moq;
using Sophrosync.Schedule.Application.Commands.CancelAppointment;
using Sophrosync.Schedule.Application.Commands.CompleteAppointment;
using Sophrosync.Schedule.Application.Commands.ConfirmAppointment;
using Sophrosync.Schedule.Application.Commands.MarkNoShow;
using Sophrosync.Schedule.Application.Commands.RescheduleAppointment;
using Sophrosync.Schedule.Application.Commands.ScheduleAppointment;
using Sophrosync.Schedule.Domain.Entities;
using Sophrosync.Schedule.Domain.Enums;
using Sophrosync.Schedule.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Schedule.Tests.Application;

public sealed class CommandHandlerTests
{
    private static readonly Guid TenantId    = Guid.NewGuid();
    private static readonly Guid ClientId    = Guid.NewGuid();
    private static readonly Guid TherapistId = Guid.NewGuid();

    private static Mock<ICurrentTenant> Tenant()
    {
        var m = new Mock<ICurrentTenant>();
        m.Setup(t => t.Id).Returns(TenantId);
        m.Setup(t => t.HasTenant).Returns(true);
        return m;
    }

    private static Mock<ICurrentTenant> NoTenant()
    {
        var m = new Mock<ICurrentTenant>();
        m.Setup(t => t.HasTenant).Returns(false);
        return m;
    }

    private static Appointment MakeAppointment() =>
        Appointment.Schedule(
            TenantId, ClientId, TherapistId,
            DateTime.UtcNow.AddDays(1), 60,
            AppointmentType.InPerson);

    // -----------------------------------------------------------------------
    // ScheduleAppointmentCommandHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ScheduleHandler_ValidCommand_ReturnsDto()
    {
        var repo = new Mock<IAppointmentRepository>();
        var handler = new ScheduleAppointmentCommandHandler(repo.Object, Tenant().Object);

        var cmd = new ScheduleAppointmentCommand(
            ClientId, TherapistId,
            DateTime.UtcNow.AddDays(1), 60,
            "InPerson", null);

        var dto = await handler.Handle(cmd, CancellationToken.None);

        dto.ClientId.Should().Be(ClientId);
        dto.TherapistId.Should().Be(TherapistId);
        dto.TenantId.Should().Be(TenantId);
        dto.Status.Should().Be("Scheduled");
        dto.Type.Should().Be("InPerson");

        repo.Verify(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ScheduleHandler_NoTenant_ThrowsAndNeverCallsRepo()
    {
        var repo = new Mock<IAppointmentRepository>();
        var handler = new ScheduleAppointmentCommandHandler(repo.Object, NoTenant().Object);

        var act = async () => await handler.Handle(
            new ScheduleAppointmentCommand(ClientId, TherapistId,
                DateTime.UtcNow.AddDays(1), 60, "InPerson", null),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        repo.Verify(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ScheduleHandler_CaseInsensitiveType_ParsesCorrectly()
    {
        var repo = new Mock<IAppointmentRepository>();
        var handler = new ScheduleAppointmentCommandHandler(repo.Object, Tenant().Object);

        var dto = await handler.Handle(
            new ScheduleAppointmentCommand(ClientId, TherapistId,
                DateTime.UtcNow.AddDays(1), 30, "video", null),
            CancellationToken.None);

        dto.Type.Should().Be("Video");
    }

    // -----------------------------------------------------------------------
    // CancelAppointmentCommandHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CancelHandler_ExistingAppointment_CancelsAndSaves()
    {
        var appt = MakeAppointment();
        var repo = new Mock<IAppointmentRepository>();
        repo.Setup(r => r.GetByIdAsync(appt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appt);

        var handler = new CancelAppointmentCommandHandler(repo.Object, Tenant().Object);
        await handler.Handle(new CancelAppointmentCommand(appt.Id, "client request"), CancellationToken.None);

        appt.Status.Should().Be(AppointmentStatus.Cancelled);
        appt.CancellationReason.Should().Be("client request");
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelHandler_NotFound_ThrowsKeyNotFound()
    {
        var repo = new Mock<IAppointmentRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        var handler = new CancelAppointmentCommandHandler(repo.Object, Tenant().Object);
        var act = async () => await handler.Handle(
            new CancelAppointmentCommand(Guid.NewGuid(), "reason"), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CancelHandler_NoTenant_ThrowsUnauthorized()
    {
        var repo = new Mock<IAppointmentRepository>();
        var handler = new CancelAppointmentCommandHandler(repo.Object, NoTenant().Object);
        var act = async () => await handler.Handle(
            new CancelAppointmentCommand(Guid.NewGuid(), "reason"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        repo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // -----------------------------------------------------------------------
    // CompleteAppointmentCommandHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CompleteHandler_ExistingAppointment_CompletesAndSaves()
    {
        var appt = MakeAppointment();
        var repo = new Mock<IAppointmentRepository>();
        repo.Setup(r => r.GetByIdAsync(appt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appt);

        var handler = new CompleteAppointmentCommandHandler(repo.Object, Tenant().Object);
        await handler.Handle(new CompleteAppointmentCommand(appt.Id, "good session"), CancellationToken.None);

        appt.Status.Should().Be(AppointmentStatus.Completed);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompleteHandler_NotFound_ThrowsKeyNotFound()
    {
        var repo = new Mock<IAppointmentRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        var handler = new CompleteAppointmentCommandHandler(repo.Object, Tenant().Object);
        var act = async () => await handler.Handle(
            new CompleteAppointmentCommand(Guid.NewGuid(), null), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CompleteHandler_NoTenant_ThrowsUnauthorized()
    {
        var repo = new Mock<IAppointmentRepository>();
        var handler = new CompleteAppointmentCommandHandler(repo.Object, NoTenant().Object);
        var act = async () => await handler.Handle(
            new CompleteAppointmentCommand(Guid.NewGuid(), null), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        repo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // -----------------------------------------------------------------------
    // ConfirmAppointmentCommandHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ConfirmHandler_ExistingAppointment_ConfirmsAndSaves()
    {
        var appt = MakeAppointment();
        var repo = new Mock<IAppointmentRepository>();
        repo.Setup(r => r.GetByIdAsync(appt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appt);

        var handler = new ConfirmAppointmentCommandHandler(repo.Object, Tenant().Object);
        await handler.Handle(new ConfirmAppointmentCommand(appt.Id), CancellationToken.None);

        appt.Status.Should().Be(AppointmentStatus.Confirmed);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConfirmHandler_NotFound_ThrowsKeyNotFound()
    {
        var repo = new Mock<IAppointmentRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        var handler = new ConfirmAppointmentCommandHandler(repo.Object, Tenant().Object);
        var act = async () => await handler.Handle(
            new ConfirmAppointmentCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ConfirmHandler_NoTenant_ThrowsUnauthorized()
    {
        var repo = new Mock<IAppointmentRepository>();
        var handler = new ConfirmAppointmentCommandHandler(repo.Object, NoTenant().Object);
        var act = async () => await handler.Handle(
            new ConfirmAppointmentCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        repo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // -----------------------------------------------------------------------
    // RescheduleAppointmentCommandHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RescheduleHandler_ExistingAppointment_ReschedulesAndSaves()
    {
        var appt = MakeAppointment();
        var repo = new Mock<IAppointmentRepository>();
        repo.Setup(r => r.GetByIdAsync(appt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appt);

        var newTime = DateTime.UtcNow.AddDays(7);
        var handler = new RescheduleAppointmentCommandHandler(repo.Object, Tenant().Object);
        await handler.Handle(new RescheduleAppointmentCommand(appt.Id, newTime, 90), CancellationToken.None);

        appt.ScheduledAt.Should().Be(newTime);
        appt.DurationMinutes.Should().Be(90);
        appt.Status.Should().Be(AppointmentStatus.Scheduled);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RescheduleHandler_NoTenant_ThrowsUnauthorized()
    {
        var repo = new Mock<IAppointmentRepository>();
        var handler = new RescheduleAppointmentCommandHandler(repo.Object, NoTenant().Object);
        var act = async () => await handler.Handle(
            new RescheduleAppointmentCommand(Guid.NewGuid(), DateTime.UtcNow.AddDays(1), null),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        repo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // -----------------------------------------------------------------------
    // MarkNoShowCommandHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task MarkNoShowHandler_ExistingAppointment_SetsNoShowAndSaves()
    {
        var appt = MakeAppointment();
        var repo = new Mock<IAppointmentRepository>();
        repo.Setup(r => r.GetByIdAsync(appt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appt);

        var handler = new MarkNoShowCommandHandler(repo.Object, Tenant().Object);
        await handler.Handle(new MarkNoShowCommand(appt.Id), CancellationToken.None);

        appt.Status.Should().Be(AppointmentStatus.NoShow);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkNoShowHandler_NoTenant_ThrowsUnauthorized()
    {
        var repo = new Mock<IAppointmentRepository>();
        var handler = new MarkNoShowCommandHandler(repo.Object, NoTenant().Object);
        var act = async () => await handler.Handle(
            new MarkNoShowCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        repo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
