using MediatR;
using Sophrosync.Schedule.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Schedule.Application.Commands.ConfirmAppointment;

public sealed class ConfirmAppointmentCommandHandler(
    IAppointmentRepository repository,
    ICurrentTenant currentTenant)
    : IRequestHandler<ConfirmAppointmentCommand, Unit>
{
    public async Task<Unit> Handle(ConfirmAppointmentCommand request, CancellationToken cancellationToken)
    {
        if (!currentTenant.HasTenant)
            throw new UnauthorizedAccessException("Tenant context is required.");

        var appointment = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Appointment {request.Id} not found.");

        appointment.Confirm();
        await repository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
