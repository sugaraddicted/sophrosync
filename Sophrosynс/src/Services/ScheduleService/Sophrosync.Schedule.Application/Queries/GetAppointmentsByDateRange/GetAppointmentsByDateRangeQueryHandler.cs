using MediatR;
using Sophrosync.Schedule.Application.DTOs;
using Sophrosync.Schedule.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Schedule.Application.Queries.GetAppointmentsByDateRange;

public sealed class GetAppointmentsByDateRangeQueryHandler(
    IAppointmentRepository repository,
    ICurrentTenant currentTenant)
    : IRequestHandler<GetAppointmentsByDateRangeQuery, IReadOnlyList<AppointmentDto>>
{
    public async Task<IReadOnlyList<AppointmentDto>> Handle(GetAppointmentsByDateRangeQuery request, CancellationToken cancellationToken)
    {
        if (!currentTenant.HasTenant)
            throw new UnauthorizedAccessException("Tenant context is required.");

        var appointments = await repository.GetByDateRangeAsync(request.From, request.To, cancellationToken);
        return appointments.Select(AppointmentDto.FromAppointment).ToList();
    }
}
