using MediatR;
using Sophrosync.Notifications.Application.DTOs;
using Sophrosync.Notifications.Domain.Interfaces;
using Sophrosync.SharedKernel.Models;

namespace Sophrosync.Notifications.Application.Queries.ListSent;

public sealed class ListSentNotificationsQueryHandler(
    INotificationRepository repository) : IRequestHandler<ListSentNotificationsQuery, PaginatedList<NotificationDto>>
{
    public async Task<PaginatedList<NotificationDto>> Handle(ListSentNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = await repository.GetSentAsync(request.Page, request.PageSize, cancellationToken);
        var dtos = notifications.Select(n => new NotificationDto(
            n.Id, n.TenantId, n.RecipientUserId, n.Channel, n.Type, n.Subject, n.Body,
            n.Status, n.RetryCount, n.ScheduledFor, n.SentAt, n.DismissedAt,
            n.CorrelationId, n.FailureReason, n.CreatedAt)).ToList();
        return new PaginatedList<NotificationDto>(dtos, dtos.Count, request.Page, request.PageSize);
    }
}
