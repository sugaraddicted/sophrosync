using MediatR;

namespace Sophrosync.Consent.Application.Commands.AttachConsentDocument;

public sealed record AttachConsentDocumentCommand(
    Guid ConsentRecordId,
    string FileName,
    string ContentType,
    byte[] Data) : IRequest<Guid>;
