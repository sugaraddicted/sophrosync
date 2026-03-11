using Microsoft.EntityFrameworkCore;
using Sophrosync.Notifications.Domain.Entities;
using Sophrosync.Notifications.Domain.Interfaces;
using Sophrosync.Notifications.Infrastructure.Persistence;

namespace Sophrosync.Notifications.Infrastructure.Repositories;

public sealed class NotificationPreferenceRepository(NotificationsDbContext context) : INotificationPreferenceRepository
{
    public async Task<NotificationPreference?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.NotificationPreferences.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task AddAsync(NotificationPreference entity, CancellationToken ct = default)
        => await context.NotificationPreferences.AddAsync(entity, ct);

    public void Update(NotificationPreference entity) => context.NotificationPreferences.Update(entity);

    public void Remove(NotificationPreference entity) => context.NotificationPreferences.Remove(entity);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await context.SaveChangesAsync(ct);

    public async Task<NotificationPreference?> GetForUserAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
        => await context.NotificationPreferences
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.UserId == userId, ct);

    public async Task<NotificationPreference?> GetTenantDefaultAsync(Guid tenantId, CancellationToken ct = default)
        => await context.NotificationPreferences
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.UserId == null, ct);
}
