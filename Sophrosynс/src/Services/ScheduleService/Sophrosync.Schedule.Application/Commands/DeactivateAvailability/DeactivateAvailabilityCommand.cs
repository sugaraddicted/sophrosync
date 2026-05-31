using MediatR;

namespace Sophrosync.Schedule.Application.Commands.DeactivateAvailability;

/// <summary>Deactivates a specific availability template by its identifier.</summary>
public sealed record DeactivateAvailabilityCommand(Guid TemplateId) : IRequest;
