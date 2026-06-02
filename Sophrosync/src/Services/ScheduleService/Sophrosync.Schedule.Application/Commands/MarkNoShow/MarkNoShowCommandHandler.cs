using MediatR;
using Sophrosync.Schedule.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Schedule.Application.Commands.MarkNoShow;

public sealed class MarkNoShowCommandHandler(
    IAppointmentRepository repository,
    ICurrentTenant currentTenant)
    : IRequestHandler<MarkNoShowCommand, Unit>
{
    public async Task<Unit> Handle(MarkNoShowCommand request, CancellationToken cancellationToken)
    {
        if (!currentTenant.HasTenant)
            throw new UnauthorizedAccessException("Tenant context is required.");

        var appointment = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Appointment {request.Id} not found.");

        appointment.MarkNoShow();
        await repository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
