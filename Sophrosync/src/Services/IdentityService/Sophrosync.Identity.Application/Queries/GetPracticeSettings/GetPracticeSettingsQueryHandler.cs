using MediatR;
using Microsoft.EntityFrameworkCore;
using Sophrosync.Identity.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Identity.Application.Queries.GetPracticeSettings;

public sealed class GetPracticeSettingsQueryHandler(
    IIdentityDbContext db,
    ICurrentTenant currentTenant)
    : IRequestHandler<GetPracticeSettingsQuery, PracticeSettingsDto>
{
    public async Task<PracticeSettingsDto> Handle(GetPracticeSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await db.PracticeSettings
            .FirstOrDefaultAsync(s => s.TenantId == currentTenant.Id, cancellationToken);

        return settings is null
            ? new PracticeSettingsDto(5, 20)
            : new PracticeSettingsDto(settings.WeeklySessionTarget, settings.MonthlySessionTarget);
    }
}
