using Sophrosync.Notifications.Domain.Entities;
using Sophrosync.Notifications.Domain.Enums;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Notifications.Domain.Interfaces;

public interface INotificationRepository : IRepository<Notification>
{
    Task<IReadOnlyList<Notification>> GetInboxAsync(Guid recipientUserId, int page, int pageSize, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(Guid recipientUserId, CancellationToken ct = default);
    Task<IReadOnlyList<Notification>> GetPendingDueAsync(DateTime asOf, CancellationToken ct = default);
    Task<IReadOnlyList<Notification>> GetFailedForRetryAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Notification>> GetSentAsync(int page, int pageSize, CancellationToken ct = default);
    Task<Notification?> GetByCorrelationIdAsync(string correlationId, CancellationToken ct = default);
}
