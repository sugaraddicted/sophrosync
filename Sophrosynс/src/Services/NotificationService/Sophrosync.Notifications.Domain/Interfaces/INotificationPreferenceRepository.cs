using Sophrosync.Notifications.Domain.Entities;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Notifications.Domain.Interfaces;

public interface INotificationPreferenceRepository : IRepository<NotificationPreference>
{
    Task<NotificationPreference?> GetForUserAsync(Guid tenantId, Guid userId, CancellationToken ct = default);
    Task<NotificationPreference?> GetTenantDefaultAsync(Guid tenantId, CancellationToken ct = default);
}
