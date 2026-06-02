using Sophrosync.SharedKernel.Domain;

namespace Sophrosync.Consent.Domain.Entities;

public sealed class ConsentDocument : Entity
{
    public Guid TenantId { get; private set; }
    public Guid ConsentRecordId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public byte[] Data { get; private set; } = [];

    private ConsentDocument() { }

    public static ConsentDocument Create(
        Guid tenantId,
        Guid consentRecordId,
        string fileName,
        string contentType,
        byte[] data)
    {
        if (data.Length > 10 * 1024 * 1024)
            throw new InvalidOperationException("Document must be smaller than 10 MB.");

        return new ConsentDocument
        {
            TenantId = tenantId,
            ConsentRecordId = consentRecordId,
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = data.Length,
            Data = data,
        };
    }
}
