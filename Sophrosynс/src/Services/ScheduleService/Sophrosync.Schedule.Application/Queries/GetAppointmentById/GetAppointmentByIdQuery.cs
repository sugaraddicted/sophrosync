using MediatR;
using Sophrosync.Schedule.Application.DTOs;

namespace Sophrosync.Schedule.Application.Queries.GetAppointmentById;

public sealed record GetAppointmentByIdQuery(Guid Id) : IRequest<AppointmentDto?>;
