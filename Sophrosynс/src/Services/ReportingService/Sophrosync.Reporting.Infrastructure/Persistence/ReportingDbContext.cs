using Microsoft.EntityFrameworkCore;
using Sophrosync.Reporting.Domain.Entities;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Reporting.Infrastructure.Persistence;

public sealed class ReportingDbContext(
    DbContextOptions<ReportingDbContext> options,
    ICurrentTenant currentTenant) : DbContext(options)
{
    public DbSet<ReportDefinition> ReportDefinitions => Set<ReportDefinition>();
    public DbSet<ReportRun> ReportRuns => Set<ReportRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReportingDbContext).Assembly);

        modelBuilder.Entity<ReportDefinition>()
            .HasQueryFilter(e => e.TenantId == currentTenant.Id);
        modelBuilder.Entity<ReportRun>()
            .HasQueryFilter(e => e.TenantId == currentTenant.Id && e.DeletedAt == null);

        base.OnModelCreating(modelBuilder);
    }
}
