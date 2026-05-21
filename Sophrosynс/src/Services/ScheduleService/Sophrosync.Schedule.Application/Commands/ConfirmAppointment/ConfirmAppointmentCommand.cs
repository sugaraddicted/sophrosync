using MediatR;

namespace Sophrosync.Schedule.Application.Commands.ConfirmAppointment;

public sealed record ConfirmAppointmentCommand(Guid Id) : IRequest<Unit>;
