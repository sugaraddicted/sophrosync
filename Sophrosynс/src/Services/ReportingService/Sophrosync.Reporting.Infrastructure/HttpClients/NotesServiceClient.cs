using System.Net.Http.Json;
using Sophrosync.Reporting.Application.DTOs;
using Sophrosync.Reporting.Application.Interfaces;

namespace Sophrosync.Reporting.Infrastructure.HttpClients;

public sealed class NotesServiceClient(HttpClient httpClient) : INotesServiceClient
{
    public string ServiceName => "NotesService";

    public async Task<NoteCompletionRateDto> GetNoteCompletionRateAsync(
        Guid tenantId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        var url = $"internal/notes/summary?tenantId={tenantId}&from={from:O}&to={to:O}";
        var result = await httpClient.GetFromJsonAsync<NoteCompletionRateDto>(url, ct);
        return result ?? new NoteCompletionRateDto(0, 0, 0, 0, 0, from, to);
    }
}
