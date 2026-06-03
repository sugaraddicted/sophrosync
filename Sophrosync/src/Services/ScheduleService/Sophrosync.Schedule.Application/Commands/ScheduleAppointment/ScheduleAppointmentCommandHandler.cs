using MediatR;
using Sophrosync.Schedule.Application.DTOs;
using Sophrosync.Schedule.Domain.Entities;
using Sophrosync.Schedule.Domain.Enums;
using Sophrosync.Schedule.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Schedule.Application.Commands.ScheduleAppointment;

public sealed class ScheduleAppointmentCommandHandler(
    IAppointmentRepository repository,
    ICurrentTenant currentTenant)
    : IRequestHandler<ScheduleAppointmentCommand, AppointmentDto>
{
    public async Task<AppointmentDto> Handle(ScheduleAppointmentCommand request, CancellationToken cancellationToken)
    {
        if (!currentTenant.HasTenant)
            throw new UnauthorizedAccessException("Tenant context is required.");

        var type = Enum.Parse<AppointmentType>(request.Type, ignoreCase: true);

        var appointment = Appointment.Schedule(
            currentTenant.Id,
            request.ClientId,
            request.TherapistId,
            request.ScheduledAt,
            request.DurationMinutes,
            type,
            request.Notes);

        await repository.AddAsync(appointment, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return AppointmentDto.FromAppointment(appointment);
    }
}
