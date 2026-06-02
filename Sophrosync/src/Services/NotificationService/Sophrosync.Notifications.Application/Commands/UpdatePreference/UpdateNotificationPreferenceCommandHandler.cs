using MediatR;
using Sophrosync.Notifications.Domain.Entities;
using Sophrosync.Notifications.Domain.Interfaces;

namespace Sophrosync.Notifications.Application.Commands.UpdatePreference;

public sealed class UpdateNotificationPreferenceCommandHandler(
    INotificationPreferenceRepository repository) : IRequestHandler<UpdateNotificationPreferenceCommand>
{
    public async Task Handle(UpdateNotificationPreferenceCommand request, CancellationToken cancellationToken)
    {
        var preference = await repository.GetForUserAsync(request.TenantId, request.UserId, cancellationToken);
        if (preference is null)
        {
            preference = NotificationPreference.Create(
                request.TenantId, request.UserId,
                request.PreferredChannel, request.EmailEnabled,
                request.InAppEnabled, request.SmsEnabled, request.EmailAddress);
            await repository.AddAsync(preference, cancellationToken);
        }
        else
        {
            preference.Update(request.PreferredChannel, request.EmailEnabled,
                request.InAppEnabled, request.SmsEnabled, request.EmailAddress);
            repository.Update(preference);
        }
        await repository.SaveChangesAsync(cancellationToken);
    }
}
