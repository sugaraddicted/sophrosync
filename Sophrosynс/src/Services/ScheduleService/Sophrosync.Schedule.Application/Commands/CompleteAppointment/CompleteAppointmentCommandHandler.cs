using MediatR;
using Sophrosync.Schedule.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Schedule.Application.Commands.CompleteAppointment;

public sealed class CompleteAppointmentCommandHandler(
    IAppointmentRepository repository,
    ICurrentTenant currentTenant)
    : IRequestHandler<CompleteAppointmentCommand, Unit>
{
    public async Task<Unit> Handle(CompleteAppointmentCommand request, CancellationToken cancellationToken)
    {
        if (!currentTenant.HasTenant)
            throw new UnauthorizedAccessException("Tenant context is required.");

        var appointment = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Appointment {request.Id} not found.");

        appointment.Complete(request.Notes);
        await repository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
