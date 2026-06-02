using Sophrosync.Reporting.Domain.Enums;
using Sophrosync.Reporting.Domain.Events;
using Sophrosync.Reporting.Domain.ValueObjects;
using Sophrosync.SharedKernel.Domain;

namespace Sophrosync.Reporting.Domain.Entities;

public sealed class ReportDefinition : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public ReportType Type { get; private set; }
    public ReportFormat Format { get; private set; }
    public ReportSchedule Schedule { get; private set; } = ReportSchedule.None();
    public DateTime? LastRunAt { get; private set; }
    public bool IsActive { get; private set; }

    private ReportDefinition() { }

    public static ReportDefinition Create(
        Guid tenantId,
        string name,
        ReportType type,
        ReportFormat format,
        ReportSchedule? schedule = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new ReportDefinition
        {
            TenantId = tenantId,
            Name = name,
            Type = type,
            Format = format,
            Schedule = schedule ?? ReportSchedule.None(),
            IsActive = true
        };
    }

    public void RecordRun()
    {
        LastRunAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
