using MediatR;
using Microsoft.EntityFrameworkCore;
using Sophrosync.Identity.Domain.Entities;
using Sophrosync.Identity.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Identity.Application.Commands.UpsertPracticeSettings;

public sealed class UpsertPracticeSettingsCommandHandler(
    IIdentityDbContext db,
    ICurrentTenant currentTenant)
    : IRequestHandler<UpsertPracticeSettingsCommand>
{
    public async Task Handle(UpsertPracticeSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await db.PracticeSettings
            .FirstOrDefaultAsync(s => s.TenantId == currentTenant.Id, cancellationToken);

        if (settings is null)
        {
            settings = PracticeSettings.CreateDefaults(currentTenant.Id);
            db.PracticeSettings.Add(settings);
        }

        settings.Update(request.WeeklySessionTarget, request.MonthlySessionTarget);
        await db.SaveChangesAsync(cancellationToken);
    }
}
