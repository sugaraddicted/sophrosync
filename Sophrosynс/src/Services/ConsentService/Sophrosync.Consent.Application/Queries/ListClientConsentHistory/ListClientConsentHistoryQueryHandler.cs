using MediatR;
using Sophrosync.Consent.Application.DTOs;
using Sophrosync.Consent.Domain.Interfaces;

namespace Sophrosync.Consent.Application.Queries.ListClientConsentHistory;

public sealed class ListClientConsentHistoryQueryHandler(
    IConsentRecordRepository repository,
    IConsentDocumentRepository documentRepository) : IRequestHandler<ListClientConsentHistoryQuery, IReadOnlyList<ConsentRecordDto>>
{
    public async Task<IReadOnlyList<ConsentRecordDto>> Handle(ListClientConsentHistoryQuery request, CancellationToken cancellationToken)
    {
        var records = await repository.GetHistoryForClientAsync(request.ClientId, cancellationToken);
        var docNames = await documentRepository.GetFileNamesByRecordIdsAsync(
            records.Select(r => r.Id), cancellationToken);

        return records.Select(r => new ConsentRecordDto(
            r.Id, r.TenantId, r.ClientId, r.ConsentRequestId,
            r.Purpose, r.Action, r.TemplateVersion, r.CreatedAt,
            docNames.GetValueOrDefault(r.Id))).ToList();
    }
}
