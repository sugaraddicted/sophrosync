using MediatR;

namespace Sophrosync.Notifications.Application.Queries.GetUnreadCount;

public sealed record GetUnreadCountQuery(Guid RecipientUserId) : IRequest<int>;
