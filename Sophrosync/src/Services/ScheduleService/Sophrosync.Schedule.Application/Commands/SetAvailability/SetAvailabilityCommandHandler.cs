using MediatR;
using Sophrosync.Schedule.Domain.Entities;
using Sophrosync.Schedule.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Schedule.Application.Commands.SetAvailability;

public sealed class SetAvailabilityCommandHandler(
    IAvailabilityTemplateRepository repository,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser)
    : IRequestHandler<SetAvailabilityCommand, Guid>
{
    public async Task<Guid> Handle(SetAvailabilityCommand request, CancellationToken cancellationToken)
    {
        if (!currentTenant.HasTenant)
            throw new UnauthorizedAccessException("Tenant context is required.");

        var therapistId = currentUser.Id;
        var tenantId = currentTenant.Id;

        // Deactivate any existing active template for the same day.
        var existing = await repository.GetByTherapistAsync(therapistId, cancellationToken);
        foreach (var template in existing.Where(t => t.DayOfWeek == request.DayOfWeek && t.IsActive))
        {
            template.Deactivate();
        }

        var newTemplate = AvailabilityTemplate.Create(
            tenantId, therapistId, request.DayOfWeek, request.StartTime, request.EndTime);

        await repository.AddAsync(newTemplate, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return newTemplate.Id;
    }
}
