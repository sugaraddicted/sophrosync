using MediatR;
using Sophrosync.Schedule.Application.DTOs;

namespace Sophrosync.Schedule.Application.Queries.GetAppointmentsByClientId;

public sealed record GetAppointmentsByClientIdQuery(Guid ClientId) : IRequest<IReadOnlyList<AppointmentDto>>;
