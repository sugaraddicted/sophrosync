using FluentValidation;

namespace Sophrosync.Schedule.Application.Commands.RescheduleAppointment;

public sealed class RescheduleAppointmentCommandValidator : AbstractValidator<RescheduleAppointmentCommand>
{
    public RescheduleAppointmentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.NewScheduledAt)
            .NotEmpty()
            .GreaterThan(DateTime.UtcNow).WithMessage("New appointment time must be in the future.");

        RuleFor(x => x.NewDurationMinutes)
            .GreaterThan(0).WithMessage("Duration must be positive.")
            .LessThanOrEqualTo(480).WithMessage("Duration may not exceed 480 minutes.")
            .When(x => x.NewDurationMinutes.HasValue);
    }
}
