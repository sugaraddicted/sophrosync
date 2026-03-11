using Sophrosync.Reporting.Domain.Entities;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Reporting.Domain.Interfaces;

public interface IReportDefinitionRepository : IRepository<ReportDefinition>
{
    Task<IReadOnlyList<ReportDefinition>> GetScheduledDueAsync(DateTime asOf, CancellationToken ct = default);
    Task<IReadOnlyList<ReportDefinition>> GetAllActiveAsync(CancellationToken ct = default);
}
