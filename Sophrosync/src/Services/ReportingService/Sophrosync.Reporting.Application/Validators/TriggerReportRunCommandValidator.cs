using FluentValidation;
using Sophrosync.Reporting.Application.Commands.TriggerReportRun;

namespace Sophrosync.Reporting.Application.Validators;

public sealed class TriggerReportRunCommandValidator : AbstractValidator<TriggerReportRunCommand>
{
    public TriggerReportRunCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ReportDefinitionId).NotEmpty();
        RuleFor(x => x.PeriodEnd).GreaterThan(x => x.PeriodStart)
            .WithMessage("PeriodEnd must be after PeriodStart.");
    }
}
