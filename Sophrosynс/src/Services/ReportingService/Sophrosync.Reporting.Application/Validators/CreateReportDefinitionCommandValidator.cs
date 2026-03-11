using FluentValidation;
using Sophrosync.Reporting.Application.Commands.CreateReportDefinition;

namespace Sophrosync.Reporting.Application.Validators;

public sealed class CreateReportDefinitionCommandValidator : AbstractValidator<CreateReportDefinitionCommand>
{
    public CreateReportDefinitionCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
