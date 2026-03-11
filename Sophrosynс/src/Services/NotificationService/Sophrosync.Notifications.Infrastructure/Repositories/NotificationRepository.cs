using Microsoft.EntityFrameworkCore;
using Sophrosync.Notifications.Domain.Entities;
using Sophrosync.Notifications.Domain.Enums;
using Sophrosync.Notifications.Domain.Interfaces;
using Sophrosync.Notifications.Infrastructure.Persistence;

namespace Sophrosync.Notifications.Infrastructure.Repositories;

public sealed class NotificationRepository(NotificationsDbContext context) : INotificationRepository
{
    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.Notifications.FirstOrDefaultAsync(n => n.Id == id, ct);

    public async Task AddAsync(Notification entity, CancellationToken ct = default)
        => await context.Notifications.AddAsync(entity, ct);

    public void Update(Notification entity) => context.Notifications.Update(entity);

    public void Remove(Notification entity) => context.Notifications.Remove(entity);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await context.SaveChangesAsync(ct);

    public async Task<IReadOnlyList<Notification>> GetInboxAsync(Guid recipientUserId, int page, int pageSize, CancellationToken ct = default)
        => await context.Notifications
            .Where(n => n.RecipientUserId == recipientUserId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<int> GetUnreadCountAsync(Guid recipientUserId, CancellationToken ct = default)
        => await context.Notifications
            .CountAsync(n => n.RecipientUserId == recipientUserId
                && n.Status == NotificationStatus.Sent
                && n.DismissedAt == null, ct);

    public async Task<IReadOnlyList<Notification>> GetPendingDueAsync(DateTime asOf, CancellationToken ct = default)
        => await context.Notifications
            .Where(n => n.Status == NotificationStatus.Pending && n.ScheduledFor <= asOf)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Notification>> GetFailedForRetryAsync(CancellationToken ct = default)
        => await context.Notifications
            .Where(n => n.Status == NotificationStatus.Failed && n.RetryCount < 3)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Notification>> GetSentAsync(int page, int pageSize, CancellationToken ct = default)
        => await context.Notifications
            .Where(n => n.Status == NotificationStatus.Sent)
            .OrderByDescending(n => n.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<Notification?> GetByCorrelationIdAsync(string correlationId, CancellationToken ct = default)
        => await context.Notifications.FirstOrDefaultAsync(n => n.CorrelationId == correlationId, ct);
}
