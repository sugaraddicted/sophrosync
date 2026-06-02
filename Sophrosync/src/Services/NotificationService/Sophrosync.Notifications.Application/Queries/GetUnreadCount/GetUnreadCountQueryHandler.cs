using MediatR;
using Sophrosync.Notifications.Domain.Interfaces;

namespace Sophrosync.Notifications.Application.Queries.GetUnreadCount;

public sealed class GetUnreadCountQueryHandler(
    INotificationRepository repository) : IRequestHandler<GetUnreadCountQuery, int>
{
    public async Task<int> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
        => await repository.GetUnreadCountAsync(request.RecipientUserId, cancellationToken);
}
