using MediatR;
using Sophrosync.Consent.Application.DTOs;
using Sophrosync.Consent.Domain.Interfaces;

namespace Sophrosync.Consent.Application.Queries.GetConsentAuditSummary;

public sealed class GetConsentAuditSummaryQueryHandler(
    IConsentRecordRepository repository) : IRequestHandler<GetConsentAuditSummaryQuery, IReadOnlyList<ConsentRecordDto>>
{
    public async Task<IReadOnlyList<ConsentRecordDto>> Handle(GetConsentAuditSummaryQuery request, CancellationToken cancellationToken)
    {
        var records = await repository.GetAllForAuditAsync(cancellationToken);
        return records.Select(r => new ConsentRecordDto(
            r.Id, r.TenantId, r.ClientId, r.ConsentRequestId,
            r.Purpose, r.Action, r.TemplateVersion, r.CreatedAt)).ToList();
    }
}
