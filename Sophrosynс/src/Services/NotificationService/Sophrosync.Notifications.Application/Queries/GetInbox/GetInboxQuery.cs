using MediatR;
using Sophrosync.Notifications.Application.DTOs;
using Sophrosync.SharedKernel.Models;

namespace Sophrosync.Notifications.Application.Queries.GetInbox;

public sealed record GetInboxQuery(Guid RecipientUserId, int Page = 1, int PageSize = 20)
    : IRequest<PaginatedList<NotificationDto>>;
