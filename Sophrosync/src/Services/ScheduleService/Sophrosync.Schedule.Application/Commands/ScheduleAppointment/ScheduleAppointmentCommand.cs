using MediatR;
using Sophrosync.Schedule.Application.DTOs;

namespace Sophrosync.Schedule.Application.Commands.ScheduleAppointment;

public sealed record ScheduleAppointmentCommand(
    Guid ClientId,
    Guid TherapistId,
    DateTime ScheduledAt,
    int DurationMinutes,
    string Type,
    string? Notes) : IRequest<AppointmentDto>;
