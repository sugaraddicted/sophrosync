using MediatR;
using Sophrosync.Consent.Domain.Interfaces;

namespace Sophrosync.Consent.Application.Queries.GetConsentDocument;

public sealed class GetConsentDocumentQueryHandler(
    IConsentDocumentRepository repository) : IRequestHandler<GetConsentDocumentQuery, ConsentDocumentDto?>
{
    public async Task<ConsentDocumentDto?> Handle(GetConsentDocumentQuery request, CancellationToken cancellationToken)
    {
        var doc = await repository.GetByRecordIdAsync(request.ConsentRecordId, cancellationToken);
        if (doc is null) return null;
        return new ConsentDocumentDto(doc.FileName, doc.ContentType, doc.Data);
    }
}
