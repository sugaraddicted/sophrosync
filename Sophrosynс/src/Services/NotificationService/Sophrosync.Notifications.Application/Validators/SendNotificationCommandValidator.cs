using FluentValidation;
using Sophrosync.Notifications.Application.Commands.SendNotification;

namespace Sophrosync.Notifications.Application.Validators;

public sealed class SendNotificationCommandValidator : AbstractValidator<SendNotificationCommand>
{
    public SendNotificationCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RecipientUserId).NotEmpty();
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Body).NotEmpty().MaximumLength(10000);
        RuleFor(x => x.CorrelationId).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ScheduledFor).GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-5))
            .WithMessage("ScheduledFor cannot be more than 5 minutes in the past.");
    }
}
