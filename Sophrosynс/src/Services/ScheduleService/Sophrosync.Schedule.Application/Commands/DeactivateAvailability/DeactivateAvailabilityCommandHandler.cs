using MediatR;
using Sophrosync.Schedule.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Schedule.Application.Commands.DeactivateAvailability;

public sealed class DeactivateAvailabilityCommandHandler(
    IAvailabilityTemplateRepository repository,
    ICurrentTenant currentTenant)
    : IRequestHandler<DeactivateAvailabilityCommand>
{
    public async Task Handle(DeactivateAvailabilityCommand request, CancellationToken cancellationToken)
    {
        if (!currentTenant.HasTenant)
            throw new UnauthorizedAccessException("Tenant context is required.");

        var template = await repository.GetByIdAsync(request.TemplateId, cancellationToken)
            ?? throw new KeyNotFoundException($"AvailabilityTemplate {request.TemplateId} was not found.");

        template.Deactivate();
        await repository.SaveChangesAsync(cancellationToken);
    }
}
