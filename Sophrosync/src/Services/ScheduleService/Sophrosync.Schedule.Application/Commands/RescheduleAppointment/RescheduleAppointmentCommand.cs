using MediatR;

namespace Sophrosync.Schedule.Application.Commands.RescheduleAppointment;

public sealed record RescheduleAppointmentCommand(
    Guid Id,
    DateTime NewScheduledAt,
    int? NewDurationMinutes) : IRequest<Unit>;
