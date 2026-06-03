using MediatR;
using Sophrosync.Schedule.Application.DTOs;
using Sophrosync.Schedule.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Schedule.Application.Queries.GetAvailabilityTemplates;

public sealed class GetAvailabilityTemplatesQueryHandler(
    IAvailabilityTemplateRepository repository,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser)
    : IRequestHandler<GetAvailabilityTemplatesQuery, IReadOnlyList<AvailabilityTemplateDto>>
{
    public async Task<IReadOnlyList<AvailabilityTemplateDto>> Handle(
        GetAvailabilityTemplatesQuery request, CancellationToken cancellationToken)
    {
        if (!currentTenant.HasTenant)
            throw new UnauthorizedAccessException("Tenant context is required.");

        var templates = await repository.GetByTherapistAsync(currentUser.Id, cancellationToken);

        return templates
            .Select(t => new AvailabilityTemplateDto(
                t.Id, t.TherapistId, t.DayOfWeek, t.StartTime, t.EndTime, t.IsActive))
            .ToList()
            .AsReadOnly();
    }
}
