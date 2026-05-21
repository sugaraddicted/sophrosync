using FluentAssertions;
using Moq;
using Sophrosync.Schedule.Application.Queries.GetAppointmentById;
using Sophrosync.Schedule.Application.Queries.GetAppointments;
using Sophrosync.Schedule.Application.Queries.GetAppointmentsByClientId;
using Sophrosync.Schedule.Application.Queries.GetAppointmentsByDateRange;
using Sophrosync.Schedule.Domain.Entities;
using Sophrosync.Schedule.Domain.Enums;
using Sophrosync.Schedule.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Schedule.Tests.Application;

public sealed class QueryHandlerTests
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

    private static Appointment MakeAppointment(Guid? clientId = null) =>
        Appointment.Schedule(
            TenantId,
            clientId ?? ClientId,
            TherapistId,
            DateTime.UtcNow.AddDays(1),
            60,
            AppointmentType.InPerson);

    // -----------------------------------------------------------------------
    // GetAppointmentsQueryHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetAppointmentsHandler_ReturnsMappedDtos()
    {
        var appts = new List<Appointment> { MakeAppointment(), MakeAppointment() };
        var repo = new Mock<IAppointmentRepository>();
        repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(appts);

        var handler = new GetAppointmentsQueryHandler(repo.Object, Tenant().Object);
        var result = await handler.Handle(new GetAppointmentsQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(d => d.Id).Should().BeEquivalentTo(appts.Select(a => a.Id));
        result.Should().AllSatisfy(d => d.Status.Should().Be("Scheduled"));
    }

    [Fact]
    public async Task GetAppointmentsHandler_EmptyRepo_ReturnsEmptyList()
    {
        var repo = new Mock<IAppointmentRepository>();
        repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment>());

        var handler = new GetAppointmentsQueryHandler(repo.Object, Tenant().Object);
        var result = await handler.Handle(new GetAppointmentsQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAppointmentsHandler_NoTenant_ThrowsAndNeverCallsRepo()
    {
        var repo = new Mock<IAppointmentRepository>();
        var handler = new GetAppointmentsQueryHandler(repo.Object, NoTenant().Object);
        var act = async () => await handler.Handle(new GetAppointmentsQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        repo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // -----------------------------------------------------------------------
    // GetAppointmentByIdQueryHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetAppointmentByIdHandler_Found_ReturnsDto()
    {
        var appt = MakeAppointment();
        var repo = new Mock<IAppointmentRepository>();
        repo.Setup(r => r.GetByIdAsync(appt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appt);

        var handler = new GetAppointmentByIdQueryHandler(repo.Object, Tenant().Object);
        var dto = await handler.Handle(new GetAppointmentByIdQuery(appt.Id), CancellationToken.None);

        dto.Should().NotBeNull();
        dto!.Id.Should().Be(appt.Id);
        dto.ClientId.Should().Be(appt.ClientId);
        dto.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public async Task GetAppointmentByIdHandler_NotFound_ReturnsNull()
    {
        var repo = new Mock<IAppointmentRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        var handler = new GetAppointmentByIdQueryHandler(repo.Object, Tenant().Object);
        var dto = await handler.Handle(new GetAppointmentByIdQuery(Guid.NewGuid()), CancellationToken.None);

        dto.Should().BeNull();
    }

    [Fact]
    public async Task GetAppointmentByIdHandler_NoTenant_ThrowsAndNeverCallsRepo()
    {
        var repo = new Mock<IAppointmentRepository>();
        var handler = new GetAppointmentByIdQueryHandler(repo.Object, NoTenant().Object);
        var act = async () => await handler.Handle(
            new GetAppointmentByIdQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        repo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // -----------------------------------------------------------------------
    // GetAppointmentsByClientIdQueryHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetByClientIdHandler_ReturnsOnlyClientAppointments()
    {
        var targetClient = Guid.NewGuid();
        var appts = new List<Appointment> { MakeAppointment(targetClient), MakeAppointment(targetClient) };
        var repo = new Mock<IAppointmentRepository>();
        repo.Setup(r => r.GetByClientIdAsync(targetClient, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appts);

        var handler = new GetAppointmentsByClientIdQueryHandler(repo.Object, Tenant().Object);
        var result = await handler.Handle(
            new GetAppointmentsByClientIdQuery(targetClient), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(d => d.ClientId.Should().Be(targetClient));
    }

    [Fact]
    public async Task GetByClientIdHandler_NoTenant_ThrowsAndNeverCallsRepo()
    {
        var repo = new Mock<IAppointmentRepository>();
        var handler = new GetAppointmentsByClientIdQueryHandler(repo.Object, NoTenant().Object);
        var act = async () => await handler.Handle(
            new GetAppointmentsByClientIdQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        repo.Verify(r => r.GetByClientIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // -----------------------------------------------------------------------
    // GetAppointmentsByDateRangeQueryHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetByDateRangeHandler_ReturnsMappedDtos()
    {
        var from  = DateTime.UtcNow.AddDays(1);
        var to    = DateTime.UtcNow.AddDays(7);
        var appts = new List<Appointment> { MakeAppointment(), MakeAppointment() };
        var repo  = new Mock<IAppointmentRepository>();
        repo.Setup(r => r.GetByDateRangeAsync(from, to, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appts);

        var handler = new GetAppointmentsByDateRangeQueryHandler(repo.Object, Tenant().Object);
        var result  = await handler.Handle(
            new GetAppointmentsByDateRangeQuery(from, to), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByDateRangeHandler_NoTenant_ThrowsAndNeverCallsRepo()
    {
        var repo    = new Mock<IAppointmentRepository>();
        var handler = new GetAppointmentsByDateRangeQueryHandler(repo.Object, NoTenant().Object);
        var act     = async () => await handler.Handle(
            new GetAppointmentsByDateRangeQuery(DateTime.UtcNow, DateTime.UtcNow.AddDays(7)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        repo.Verify(r => r.GetByDateRangeAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
