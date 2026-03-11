using System.Net.Http.Json;
using Sophrosync.Reporting.Application.DTOs;
using Sophrosync.Reporting.Application.Interfaces;

namespace Sophrosync.Reporting.Infrastructure.HttpClients;

public sealed class ScheduleServiceClient(HttpClient httpClient) : IScheduleServiceClient
{
    public string ServiceName => "ScheduleService";

    public async Task<AppointmentSummaryDto> GetAppointmentSummaryAsync(
        Guid tenantId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        var url = $"internal/appointments/summary?tenantId={tenantId}&from={from:O}&to={to:O}";
        var result = await httpClient.GetFromJsonAsync<AppointmentSummaryDto>(url, ct);
        return result ?? new AppointmentSummaryDto(0, 0, 0, 0, from, to);
    }
}
