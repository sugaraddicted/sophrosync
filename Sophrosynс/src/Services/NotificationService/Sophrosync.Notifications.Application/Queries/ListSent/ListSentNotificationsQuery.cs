using MediatR;
using Sophrosync.Notifications.Application.DTOs;
using Sophrosync.SharedKernel.Models;

namespace Sophrosync.Notifications.Application.Queries.ListSent;

public sealed record ListSentNotificationsQuery(int Page = 1, int PageSize = 20)
    : IRequest<PaginatedList<NotificationDto>>;
