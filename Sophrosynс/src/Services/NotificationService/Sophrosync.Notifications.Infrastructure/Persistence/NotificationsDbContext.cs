using Microsoft.EntityFrameworkCore;
using Sophrosync.Notifications.Domain.Entities;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Notifications.Infrastructure.Persistence;

public sealed class NotificationsDbContext(
    DbContextOptions<NotificationsDbContext> options,
    ICurrentTenant currentTenant) : DbContext(options)
{
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationsDbContext).Assembly);

        modelBuilder.Entity<Notification>()
            .HasQueryFilter(e => e.TenantId == currentTenant.Id && e.DeletedAt == null);
        modelBuilder.Entity<NotificationPreference>()
            .HasQueryFilter(e => e.TenantId == currentTenant.Id);

        base.OnModelCreating(modelBuilder);
    }
}
