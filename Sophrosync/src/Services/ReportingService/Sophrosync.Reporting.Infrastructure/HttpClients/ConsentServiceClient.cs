using System.Net.Http.Json;
using Sophrosync.Reporting.Application.Interfaces;

namespace Sophrosync.Reporting.Infrastructure.HttpClients;

public sealed class ConsentServiceClient(HttpClient httpClient) : IConsentServiceClient
{
    public string ServiceName => "ConsentService";

    public async Task<bool> GetConsentStatusAsync(
        Guid tenantId, Guid clientId, string purpose, CancellationToken ct = default)
    {
        var url = $"internal/consent/status?tenantId={tenantId}&clientId={clientId}&purpose={purpose}";
        var result = await httpClient.GetFromJsonAsync<ConsentStatusResponse>(url, ct);
        return result?.HasActiveConsent ?? false;
    }

    private sealed record ConsentStatusResponse(bool HasActiveConsent);
}
