using MediatR;
using Sophrosync.Notifications.Domain.Entities;
using Sophrosync.Notifications.Domain.Enums;
using Sophrosync.Notifications.Domain.Interfaces;

namespace Sophrosync.Notifications.Application.Commands.SendNotification;

public sealed class SendNotificationCommandHandler(
    INotificationRepository repository,
    INotificationPreferenceRepository preferenceRepository,
    IPublisher publisher) : IRequestHandler<SendNotificationCommand, Guid>
{
    public async Task<Guid> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        // Idempotency: return existing if already created
        var existing = await repository.GetByCorrelationIdAsync(request.CorrelationId, cancellationToken);
        if (existing is not null) return existing.Id;

        var prefs = await preferenceRepository.GetForUserAsync(request.TenantId, request.RecipientUserId, cancellationToken);

        Guid primaryId = Guid.Empty;

        // In-app notification (default on; disabled only when prefs explicitly set InAppEnabled=false)
        if (prefs is null || prefs.InAppEnabled)
        {
            var inApp = Notification.Create(
                request.TenantId,
                request.RecipientUserId,
                NotificationChannel.InApp,
                request.Type,
                request.Subject,
                request.Body,
                request.ScheduledFor,
                request.CorrelationId);

            await repository.AddAsync(inApp, cancellationToken);
            primaryId = inApp.Id;

            foreach (var evt in inApp.DomainEvents)
                await publisher.Publish(evt, cancellationToken);
            inApp.ClearDomainEvents();
        }

        // Email fan-out: only when prefs explicitly enable email and an address is saved
        if (prefs is { EmailEnabled: true } && !string.IsNullOrWhiteSpace(prefs.EmailAddress))
        {
            var emailCorrelationId = request.CorrelationId + ":email";
            var existingEmail = await repository.GetByCorrelationIdAsync(emailCorrelationId, cancellationToken);
            if (existingEmail is null)
            {
                var emailNotif = Notification.Create(
                    request.TenantId,
                    request.RecipientUserId,
                    NotificationChannel.Email,
                    request.Type,
                    request.Subject,
                    request.Body,
                    request.ScheduledFor,
                    emailCorrelationId);

                await repository.AddAsync(emailNotif, cancellationToken);
                if (primaryId == Guid.Empty) primaryId = emailNotif.Id;

                foreach (var evt in emailNotif.DomainEvents)
                    await publisher.Publish(evt, cancellationToken);
                emailNotif.ClearDomainEvents();
            }
        }

        await repository.SaveChangesAsync(cancellationToken);
        return primaryId;
    }
}
