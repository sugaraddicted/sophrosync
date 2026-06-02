using MediatR;
using Sophrosync.Consent.Domain.Entities;
using Sophrosync.Consent.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Consent.Application.Commands.AttachConsentDocument;

public sealed class AttachConsentDocumentCommandHandler(
    IConsentRecordRepository recordRepository,
    IConsentDocumentRepository documentRepository,
    ICurrentTenant currentTenant) : IRequestHandler<AttachConsentDocumentCommand, Guid>
{
    public async Task<Guid> Handle(AttachConsentDocumentCommand request, CancellationToken cancellationToken)
    {
        _ = await recordRepository.GetByIdAsync(request.ConsentRecordId, cancellationToken)
            ?? throw new KeyNotFoundException($"ConsentRecord {request.ConsentRecordId} not found.");

        var existing = await documentRepository.GetByRecordIdAsync(request.ConsentRecordId, cancellationToken);
        if (existing is not null)
        {
            documentRepository.Remove(existing);
            await documentRepository.SaveChangesAsync(cancellationToken);
        }

        var document = ConsentDocument.Create(
            currentTenant.Id,
            request.ConsentRecordId,
            request.FileName,
            request.ContentType,
            request.Data);

        await documentRepository.AddAsync(document, cancellationToken);
        await documentRepository.SaveChangesAsync(cancellationToken);

        return document.Id;
    }
}
