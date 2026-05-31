using Microsoft.EntityFrameworkCore;
using Sophrosync.Schedule.Domain.Entities;
using Sophrosync.Schedule.Domain.Interfaces;
using Sophrosync.Schedule.Infrastructure.Persistence;

namespace Sophrosync.Schedule.Infrastructure.Persistence.Repositories;

public sealed class AvailabilityTemplateRepository(ScheduleDbContext db) : IAvailabilityTemplateRepository
{
    public async Task<IReadOnlyList<AvailabilityTemplate>> GetByTherapistAsync(
        Guid therapistId, CancellationToken ct)
        => await db.AvailabilityTemplates
            .Where(t => t.TherapistId == therapistId)
            .OrderBy(t => t.DayOfWeek)
            .ToListAsync(ct);

    public Task AddAsync(AvailabilityTemplate template, CancellationToken ct)
    {
        db.AvailabilityTemplates.Add(template);
        return Task.CompletedTask;
    }

    public async Task<AvailabilityTemplate?> GetByIdAsync(Guid id, CancellationToken ct)
        => await db.AvailabilityTemplates.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task SaveChangesAsync(CancellationToken ct)
        => await db.SaveChangesAsync(ct);
}
