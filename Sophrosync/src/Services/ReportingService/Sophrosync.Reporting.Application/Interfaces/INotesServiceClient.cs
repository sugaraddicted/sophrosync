using Sophrosync.Reporting.Application.DTOs;
using Sophrosync.SharedKernel.Http;

namespace Sophrosync.Reporting.Application.Interfaces;

public interface INotesServiceClient : ITypedServiceClient
{
    Task<NoteCompletionRateDto> GetNoteCompletionRateAsync(Guid tenantId, DateTime from, DateTime to, CancellationToken ct = default);
}
