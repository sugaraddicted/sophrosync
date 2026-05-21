using MediatR;
using Sophrosync.Schedule.Application.DTOs;

namespace Sophrosync.Schedule.Application.Queries.GetAppointments;

public sealed record GetAppointmentsQuery : IRequest<IReadOnlyList<AppointmentDto>>;
