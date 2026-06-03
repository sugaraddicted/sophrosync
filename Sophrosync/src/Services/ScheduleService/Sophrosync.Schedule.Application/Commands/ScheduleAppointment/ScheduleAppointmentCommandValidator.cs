using FluentValidation;
using Sophrosync.Schedule.Domain.Enums;

namespace Sophrosync.Schedule.Application.Commands.ScheduleAppointment;

public sealed class ScheduleAppointmentCommandValidator : AbstractValidator<ScheduleAppointmentCommand>
{
    public ScheduleAppointmentCommandValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.TherapistId).NotEmpty();

        RuleFor(x => x.ScheduledAt)
            .NotEmpty()
            .GreaterThan(DateTime.UtcNow).WithMessage("Appointment must be scheduled in the future.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("Duration must be positive.")
            .LessThanOrEqualTo(480).WithMessage("Duration may not exceed 480 minutes.");

        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => Enum.TryParse<AppointmentType>(t, ignoreCase: true, out _))
            .WithMessage($"Type must be one of: {string.Join(", ", Enum.GetNames<AppointmentType>())}.");

        RuleFor(x => x.Notes)
            .MaximumLength(5000)
            .When(x => x.Notes is not null);
    }
}
