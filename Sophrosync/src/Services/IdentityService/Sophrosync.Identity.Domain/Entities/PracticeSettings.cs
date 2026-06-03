using Sophrosync.SharedKernel.Domain;

namespace Sophrosync.Identity.Domain.Entities;

public sealed class PracticeSettings : Entity
{
    public Guid TenantId { get; private set; }
    public int WeeklySessionTarget { get; private set; }
    public int MonthlySessionTarget { get; private set; }

    private PracticeSettings() { }

    public static PracticeSettings CreateDefaults(Guid tenantId) => new()
    {
        TenantId = tenantId,
        WeeklySessionTarget = 5,
        MonthlySessionTarget = 20,
    };

    public void Update(int weeklyTarget, int monthlyTarget)
    {
        WeeklySessionTarget = weeklyTarget;
        MonthlySessionTarget = monthlyTarget;
    }
}
