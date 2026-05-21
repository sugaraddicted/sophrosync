using MediatR;
using Sophrosync.Schedule.Application.DTOs;
using Sophrosync.Schedule.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Schedule.Application.Queries.GetAppointmentById;

public sealed class GetAppointmentByIdQueryHandler(
    IAppointmentRepository repository,
    ICurrentTenant currentTenant)
    : IRequestHandler<GetAppointmentByIdQuery, AppointmentDto?>
{
    public async Task<AppointmentDto?> Handle(GetAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        if (!currentTenant.HasTenant)
            throw new UnauthorizedAccessException("Tenant context is required.");

        var appointment = await repository.GetByIdAsync(request.Id, cancellationToken);
        return appointment is null ? null : AppointmentDto.FromAppointment(appointment);
    }
}
