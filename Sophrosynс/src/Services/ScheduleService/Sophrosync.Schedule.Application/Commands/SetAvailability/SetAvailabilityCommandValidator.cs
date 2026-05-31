using FluentValidation;

namespace Sophrosync.Schedule.Application.Commands.SetAvailability;

public sealed class SetAvailabilityCommandValidator : AbstractValidator<SetAvailabilityCommand>
{
    public SetAvailabilityCommandValidator()
    {
        RuleFor(x => x.DayOfWeek)
            .IsInEnum()
            .WithMessage("DayOfWeek must be a valid value between 0 (Sunday) and 6 (Saturday).");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("EndTime must be after StartTime.");
    }
}
