using Sophrosync.Schedule.Domain.Entities;

namespace Sophrosync.Schedule.Domain.Interfaces;

/// <summary>
/// Persistence contract for <see cref="AvailabilityTemplate"/> aggregates.
/// </summary>
public interface IAvailabilityTemplateRepository
{
    Task<IReadOnlyList<AvailabilityTemplate>> GetByTherapistAsync(Guid therapistId, CancellationToken ct);
    Task AddAsync(AvailabilityTemplate template, CancellationToken ct);
    Task<AvailabilityTemplate?> GetByIdAsync(Guid id, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
