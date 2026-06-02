using MediatR;
using Sophrosync.Notifications.Application.DTOs;
using Sophrosync.Notifications.Domain.Interfaces;

namespace Sophrosync.Notifications.Application.Queries.GetById;

public sealed class GetNotificationByIdQueryHandler(
    INotificationRepository repository) : IRequestHandler<GetNotificationByIdQuery, NotificationDto?>
{
    public async Task<NotificationDto?> Handle(GetNotificationByIdQuery request, CancellationToken cancellationToken)
    {
        var n = await repository.GetByIdAsync(request.NotificationId, cancellationToken);
        if (n is null) return null;
        return new NotificationDto(n.Id, n.TenantId, n.RecipientUserId, n.Channel, n.Type,
            n.Subject, n.Body, n.Status, n.RetryCount, n.ScheduledFor, n.SentAt,
            n.DismissedAt, n.CorrelationId, n.FailureReason, n.CreatedAt);
    }
}
