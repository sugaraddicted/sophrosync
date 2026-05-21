using Microsoft.EntityFrameworkCore;
using Sophrosync.Schedule.Domain.Entities;
using Sophrosync.Schedule.Infrastructure.Persistence.Configurations;
using Sophrosync.SharedKernel.Abstractions;
using Sophrosync.SharedKernel.Domain;

namespace Sophrosync.Schedule.Infrastructure.Persistence;

public sealed class ScheduleDbContext : DbContext
{
    private readonly Guid _tenantId;

    public ScheduleDbContext(
        DbContextOptions<ScheduleDbContext> options,
        ICurrentTenant currentTenant) : base(options)
    {
        _tenantId = currentTenant.Id;
    }

    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AppointmentConfiguration());

        // Value is resolved from a per-instance field — safe under DbContext pooling.
        modelBuilder.Entity<Appointment>()
            .HasQueryFilter(e => e.TenantId == _tenantId);

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<Entity>()
            .Where(e => e.State == EntityState.Modified))
        {
            entry.Entity.TouchUpdatedAt(utcNow);
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
