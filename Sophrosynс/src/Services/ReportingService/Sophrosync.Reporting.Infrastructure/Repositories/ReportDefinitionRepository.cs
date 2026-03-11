using Microsoft.EntityFrameworkCore;
using Sophrosync.Reporting.Domain.Entities;
using Sophrosync.Reporting.Domain.Interfaces;
using Sophrosync.Reporting.Infrastructure.Persistence;

namespace Sophrosync.Reporting.Infrastructure.Repositories;

public sealed class ReportDefinitionRepository(ReportingDbContext context) : IReportDefinitionRepository
{
    public async Task<ReportDefinition?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.ReportDefinitions.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task AddAsync(ReportDefinition entity, CancellationToken ct = default)
        => await context.ReportDefinitions.AddAsync(entity, ct);

    public void Update(ReportDefinition entity) => context.ReportDefinitions.Update(entity);

    public void Remove(ReportDefinition entity) => context.ReportDefinitions.Remove(entity);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await context.SaveChangesAsync(ct);

    public async Task<IReadOnlyList<ReportDefinition>> GetScheduledDueAsync(DateTime asOf, CancellationToken ct = default)
        => await context.ReportDefinitions
            .Where(r => r.IsActive && r.Schedule.IsScheduled
                && (r.LastRunAt == null || r.LastRunAt < asOf.AddHours(-23)))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ReportDefinition>> GetAllActiveAsync(CancellationToken ct = default)
        => await context.ReportDefinitions.Where(r => r.IsActive).ToListAsync(ct);
}
