using Sophrosync.Consent.Domain.Entities;

namespace Sophrosync.Consent.Domain.Interfaces;

public interface IConsentDocumentRepository
{
    Task<ConsentDocument?> GetByRecordIdAsync(Guid consentRecordId, CancellationToken ct = default);
    Task<IReadOnlyDictionary<Guid, string>> GetFileNamesByRecordIdsAsync(IEnumerable<Guid> recordIds, CancellationToken ct = default);
    Task AddAsync(ConsentDocument document, CancellationToken ct = default);
    void Remove(ConsentDocument document);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
