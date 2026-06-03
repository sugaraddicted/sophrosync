using MediatR;

namespace Sophrosync.Identity.Application.Queries.GetPracticeSettings;

public sealed record GetPracticeSettingsQuery : IRequest<PracticeSettingsDto>;

public sealed record PracticeSettingsDto(int WeeklySessionTarget, int MonthlySessionTarget);
