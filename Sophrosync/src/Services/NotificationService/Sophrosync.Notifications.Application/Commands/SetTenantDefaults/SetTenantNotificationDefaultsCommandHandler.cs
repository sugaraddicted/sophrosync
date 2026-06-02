using MediatR;
using Sophrosync.Notifications.Domain.Entities;
using Sophrosync.Notifications.Domain.Interfaces;

namespace Sophrosync.Notifications.Application.Commands.SetTenantDefaults;

public sealed class SetTenantNotificationDefaultsCommandHandler(
    INotificationPreferenceRepository repository) : IRequestHandler<SetTenantNotificationDefaultsCommand>
{
    public async Task Handle(SetTenantNotificationDefaultsCommand request, CancellationToken cancellationToken)
    {
        var tenantDefault = await repository.GetTenantDefaultAsync(request.TenantId, cancellationToken);
        if (tenantDefault is null)
        {
            tenantDefault = NotificationPreference.Create(
                request.TenantId, null,
                request.PreferredChannel, request.EmailEnabled,
                request.InAppEnabled, request.SmsEnabled);
            await repository.AddAsync(tenantDefault, cancellationToken);
        }
        else
        {
            tenantDefault.Update(request.PreferredChannel, request.EmailEnabled,
                request.InAppEnabled, request.SmsEnabled, null);
            repository.Update(tenantDefault);
        }
        await repository.SaveChangesAsync(cancellationToken);
    }
}
