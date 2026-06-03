using MediatR;

namespace Sophrosync.Schedule.Application.Commands.SetAvailability;

/// <summary>
/// Sets (or replaces) the availability template for the requesting therapist on a given day of week.
/// Any existing active template for the same day is deactivated.
/// </summary>
public sealed record SetAvailabilityCommand(
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime) : IRequest<Guid>;
