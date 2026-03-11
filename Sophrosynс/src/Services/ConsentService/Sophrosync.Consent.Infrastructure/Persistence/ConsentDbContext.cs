using Microsoft.EntityFrameworkCore;
using Sophrosync.Consent.Domain.Entities;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Consent.Infrastructure.Persistence;

public sealed class ConsentDbContext(
    DbContextOptions<ConsentDbContext> options,
    ICurrentTenant currentTenant) : DbContext(options)
{
    public DbSet<ConsentTemplate> ConsentTemplates => Set<ConsentTemplate>();
    public DbSet<ConsentRequest> ConsentRequests => Set<ConsentRequest>();
    public DbSet<ConsentRecord> ConsentRecords => Set<ConsentRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConsentDbContext).Assembly);

        modelBuilder.Entity<ConsentTemplate>()
            .HasQueryFilter(e => e.TenantId == currentTenant.Id);
        modelBuilder.Entity<ConsentRequest>()
            .HasQueryFilter(e => e.TenantId == currentTenant.Id);
        modelBuilder.Entity<ConsentRecord>()
            .HasQueryFilter(e => e.TenantId == currentTenant.Id);

        base.OnModelCreating(modelBuilder);
    }
}
