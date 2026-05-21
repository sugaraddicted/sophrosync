using MediatR;
using Sophrosync.Schedule.Application.DTOs;
using Sophrosync.Schedule.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Schedule.Application.Queries.GetAppointmentsByClientId;

public sealed class GetAppointmentsByClientIdQueryHandler(
    IAppointmentRepository repository,
    ICurrentTenant currentTenant)
    : IRequestHandler<GetAppointmentsByClientIdQuery, IReadOnlyList<AppointmentDto>>
{
    public async Task<IReadOnlyList<AppointmentDto>> Handle(GetAppointmentsByClientIdQuery request, CancellationToken cancellationToken)
    {
        if (!currentTenant.HasTenant)
            throw new UnauthorizedAccessException("Tenant context is required.");

        var appointments = await repository.GetByClientIdAsync(request.ClientId, cancellationToken);
        return appointments.Select(AppointmentDto.FromAppointment).ToList();
    }
}
