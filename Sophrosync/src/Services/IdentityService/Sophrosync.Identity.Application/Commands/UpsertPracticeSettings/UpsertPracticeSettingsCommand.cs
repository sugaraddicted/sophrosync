using MediatR;

namespace Sophrosync.Identity.Application.Commands.UpsertPracticeSettings;

public sealed record UpsertPracticeSettingsCommand(
    int WeeklySessionTarget,
    int MonthlySessionTarget) : IRequest;
