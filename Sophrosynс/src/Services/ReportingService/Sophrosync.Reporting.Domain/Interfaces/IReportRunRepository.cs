using Sophrosync.Reporting.Domain.Entities;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Reporting.Domain.Interfaces;

public interface IReportRunRepository : IRepository<ReportRun>
{
    Task<IReadOnlyList<ReportRun>> GetForDefinitionAsync(Guid definitionId, CancellationToken ct = default);
}
