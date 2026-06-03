using MediatR;

namespace Sophrosync.Schedule.Application.Commands.CompleteAppointment;

public sealed record CompleteAppointmentCommand(Guid Id, string? Notes) : IRequest<Unit>;
