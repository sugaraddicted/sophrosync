using MediatR;
using Sophrosync.Notifications.Application.DTOs;
using Sophrosync.Notifications.Domain.Interfaces;
using Sophrosync.SharedKernel.Models;

namespace Sophrosync.Notifications.Application.Queries.GetInbox;

public sealed class GetInboxQueryHandler(
    INotificationRepository repository) : IRequestHandler<GetInboxQuery, PaginatedList<NotificationDto>>
{
    public async Task<PaginatedList<NotificationDto>> Handle(GetInboxQuery request, CancellationToken cancellationToken)
    {
        var notifications = await repository.GetInboxAsync(request.RecipientUserId, request.Page, request.PageSize, cancellationToken);
        var dtos = notifications.Select(n => new NotificationDto(
            n.Id, n.TenantId, n.RecipientUserId, n.Channel, n.Type, n.Subject, n.Body,
            n.Status, n.RetryCount, n.ScheduledFor, n.SentAt, n.DismissedAt,
            n.CorrelationId, n.FailureReason, n.CreatedAt)).ToList();
        return new PaginatedList<NotificationDto>(dtos, dtos.Count, request.Page, request.PageSize);
    }
}
