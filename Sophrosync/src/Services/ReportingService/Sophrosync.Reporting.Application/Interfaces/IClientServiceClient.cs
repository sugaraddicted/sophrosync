using Sophrosync.Reporting.Application.DTOs;
using Sophrosync.SharedKernel.Http;

namespace Sophrosync.Reporting.Application.Interfaces;

public interface IClientServiceClient : ITypedServiceClient
{
    Task<ClinicalOutcomeSummaryDto> GetClientSummaryAsync(Guid tenantId, DateTime from, DateTime to, CancellationToken ct = default);
}
