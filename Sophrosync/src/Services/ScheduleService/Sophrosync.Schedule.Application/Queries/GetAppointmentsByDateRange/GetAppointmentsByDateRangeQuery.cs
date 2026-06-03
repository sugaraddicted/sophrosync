using MediatR;
using Sophrosync.Schedule.Application.DTOs;

namespace Sophrosync.Schedule.Application.Queries.GetAppointmentsByDateRange;

public sealed record GetAppointmentsByDateRangeQuery(DateTime From, DateTime To) : IRequest<IReadOnlyList<AppointmentDto>>;
