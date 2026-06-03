using MediatR;
using Sophrosync.Reporting.Application.DTOs;
using Sophrosync.Reporting.Application.Interfaces;

namespace Sophrosync.Reporting.Application.Queries.GetNoteCompletionRate;

public sealed class GetNoteCompletionRateQueryHandler(
    INotesServiceClient notesServiceClient) : IRequestHandler<GetNoteCompletionRateQuery, NoteCompletionRateDto>
{
    public async Task<NoteCompletionRateDto> Handle(GetNoteCompletionRateQuery request, CancellationToken cancellationToken)
        => await notesServiceClient.GetNoteCompletionRateAsync(
            request.TenantId, request.PeriodStart, request.PeriodEnd, cancellationToken);
}
