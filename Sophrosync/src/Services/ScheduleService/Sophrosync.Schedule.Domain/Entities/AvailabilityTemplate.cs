using Sophrosync.SharedKernel.Domain;

namespace Sophrosync.Schedule.Domain.Entities;

/// <summary>
/// Represents a recurring weekly availability window for a therapist.
/// </summary>
public sealed class AvailabilityTemplate : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid TherapistId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public bool IsActive { get; private set; }

    private AvailabilityTemplate() { }

    /// <summary>
    /// Creates a new active availability template, enforcing that EndTime is after StartTime.
    /// </summary>
    public static AvailabilityTemplate Create(
        Guid tenantId, Guid therapistId, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        if (endTime <= startTime)
            throw new ArgumentException("EndTime must be after StartTime.");

        return new AvailabilityTemplate
        {
            TenantId = tenantId,
            TherapistId = therapistId,
            DayOfWeek = dayOfWeek,
            StartTime = startTime,
            EndTime = endTime,
            IsActive = true,
        };
    }

    /// <summary>Marks this template as inactive.</summary>
    public void Deactivate() => IsActive = false;

    /// <summary>Marks this template as active.</summary>
    public void Activate() => IsActive = true;
}
