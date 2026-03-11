using Sophrosync.Reporting.Domain.Enums;
using Sophrosync.Reporting.Domain.Events;
using Sophrosync.SharedKernel.Domain;

namespace Sophrosync.Reporting.Domain.Entities;

public sealed class ReportRun : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid ReportDefinitionId { get; private set; }
    public Guid RequestedByUserId { get; private set; }
    public ReportRunStatus Status { get; private set; }
    public string? ResultJson { get; private set; } // AES-256-GCM encrypted
    public string? FailureReason { get; private set; }
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private ReportRun() { }

    public static ReportRun Create(
        Guid tenantId,
        Guid reportDefinitionId,
        Guid requestedByUserId,
        DateTime periodStart,
        DateTime periodEnd)
    {
        var run = new ReportRun
        {
            TenantId = tenantId,
            ReportDefinitionId = reportDefinitionId,
            RequestedByUserId = requestedByUserId,
            Status = ReportRunStatus.Queued,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd
        };

        run.RaiseDomainEvent(new ReportRunCreatedDomainEvent(run.Id, tenantId, reportDefinitionId));
        return run;
    }

    public void MarkRunning()
    {
        Status = ReportRunStatus.Running;
    }

    public void Complete(string resultJson)
    {
        Status = ReportRunStatus.Completed;
        ResultJson = resultJson;
        CompletedAt = DateTime.UtcNow;
        RaiseDomainEvent(new ReportRunCompletedDomainEvent(Id, TenantId));
    }

    public void Fail(string reason)
    {
        Status = ReportRunStatus.Failed;
        FailureReason = reason;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// GDPR data minimisation — nulls result payload, keeps metadata for audit.
    /// </summary>
    public void DeleteResult()
    {
        ResultJson = null;
        DeletedAt = DateTime.UtcNow;
    }
}
