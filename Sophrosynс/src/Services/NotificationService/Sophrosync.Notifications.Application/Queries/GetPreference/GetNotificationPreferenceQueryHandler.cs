using MediatR;
using Sophrosync.Notifications.Application.DTOs;
using Sophrosync.Notifications.Domain.Interfaces;

namespace Sophrosync.Notifications.Application.Queries.GetPreference;

public sealed class GetNotificationPreferenceQueryHandler(
    INotificationPreferenceRepository repository) : IRequestHandler<GetNotificationPreferenceQuery, NotificationPreferenceDto?>
{
    public async Task<NotificationPreferenceDto?> Handle(GetNotificationPreferenceQuery request, CancellationToken cancellationToken)
    {
        var p = await repository.GetForUserAsync(request.TenantId, request.UserId, cancellationToken);
        if (p is null) return null;
        return new NotificationPreferenceDto(p.Id, p.TenantId, p.UserId,
            p.PreferredChannel, p.EmailEnabled, p.InAppEnabled, p.SmsEnabled, p.EmailAddress);
    }
}
