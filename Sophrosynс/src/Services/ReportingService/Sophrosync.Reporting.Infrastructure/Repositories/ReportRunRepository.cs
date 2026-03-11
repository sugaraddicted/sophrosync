using Microsoft.EntityFrameworkCore;
using Sophrosync.Reporting.Domain.Entities;
using Sophrosync.Reporting.Domain.Interfaces;
using Sophrosync.Reporting.Infrastructure.Persistence;

namespace Sophrosync.Reporting.Infrastructure.Repositories;

public sealed class ReportRunRepository(ReportingDbContext context) : IReportRunRepository
{
    public async Task<ReportRun?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.ReportRuns.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task AddAsync(ReportRun entity, CancellationToken ct = default)
        => await context.ReportRuns.AddAsync(entity, ct);

    public void Update(ReportRun entity) => context.ReportRuns.Update(entity);

    public void Remove(ReportRun entity) => context.ReportRuns.Remove(entity);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await context.SaveChangesAsync(ct);

    public async Task<IReadOnlyList<ReportRun>> GetForDefinitionAsync(Guid definitionId, CancellationToken ct = default)
        => await context.ReportRuns
            .Where(r => r.ReportDefinitionId == definitionId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
}
