using System.Net.Http.Json;
using Sophrosync.Reporting.Application.DTOs;
using Sophrosync.Reporting.Application.Interfaces;

namespace Sophrosync.Reporting.Infrastructure.HttpClients;

public sealed class ClientServiceClient(HttpClient httpClient) : IClientServiceClient
{
    public string ServiceName => "ClientService";

    public async Task<ClinicalOutcomeSummaryDto> GetClientSummaryAsync(
        Guid tenantId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        var url = $"internal/clients/summary?tenantId={tenantId}&from={from:O}&to={to:O}";
        var result = await httpClient.GetFromJsonAsync<ClinicalOutcomeSummaryDto>(url, ct);
        return result ?? new ClinicalOutcomeSummaryDto(tenantId, from, to, 0, 0, 0, 0, 0, 0);
    }
}
