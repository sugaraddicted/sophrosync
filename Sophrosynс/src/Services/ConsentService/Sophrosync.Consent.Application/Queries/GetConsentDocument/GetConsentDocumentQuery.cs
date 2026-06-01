using MediatR;

namespace Sophrosync.Consent.Application.Queries.GetConsentDocument;

public sealed record GetConsentDocumentQuery(Guid ConsentRecordId) : IRequest<ConsentDocumentDto?>;

public sealed record ConsentDocumentDto(string FileName, string ContentType, byte[] Data);
