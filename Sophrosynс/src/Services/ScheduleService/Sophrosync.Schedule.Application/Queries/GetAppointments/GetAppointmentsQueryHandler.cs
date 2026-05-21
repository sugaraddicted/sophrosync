using MediatR;
using Sophrosync.Schedule.Application.DTOs;
using Sophrosync.Schedule.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Schedule.Application.Queries.GetAppointments;

public sealed class GetAppointmentsQueryHandler(
    IAppointmentRepository repository,
    ICurrentTenant currentTenant)
    : IRequestHandler<GetAppointmentsQuery, IReadOnlyList<AppointmentDto>>
{
    public async Task<IReadOnlyList<AppointmentDto>> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        if (!currentTenant.HasTenant)
            throw new UnauthorizedAccessException("Tenant context is required.");

        var appointments = await repository.GetAllAsync(cancellationToken);
        return appointments.Select(AppointmentDto.FromAppointment).ToList();
    }
}
