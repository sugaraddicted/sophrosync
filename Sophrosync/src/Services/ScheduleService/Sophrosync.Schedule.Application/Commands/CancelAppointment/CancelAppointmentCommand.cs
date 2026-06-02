using MediatR;

namespace Sophrosync.Schedule.Application.Commands.CancelAppointment;

public sealed record CancelAppointmentCommand(Guid Id, string Reason) : IRequest<Unit>;
