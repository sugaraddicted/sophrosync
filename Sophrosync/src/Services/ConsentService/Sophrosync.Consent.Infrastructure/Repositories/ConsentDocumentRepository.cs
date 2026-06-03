using Microsoft.EntityFrameworkCore;
using Sophrosync.Consent.Domain.Entities;
using Sophrosync.Consent.Domain.Interfaces;
using Sophrosync.Consent.Infrastructure.Persistence;

namespace Sophrosync.Consent.Infrastructure.Repositories;

public sealed class ConsentDocumentRepository(ConsentDbContext context) : IConsentDocumentRepository
{
    public Task<ConsentDocument?> GetByRecordIdAsync(Guid consentRecordId, CancellationToken ct = default)
        => context.ConsentDocuments.FirstOrDefaultAsync(d => d.ConsentRecordId == consentRecordId, ct);

    public async Task<IReadOnlyDictionary<Guid, string>> GetFileNamesByRecordIdsAsync(
        IEnumerable<Guid> recordIds, CancellationToken ct = default)
    {
        var ids = recordIds.ToList();
        if (ids.Count == 0) return new Dictionary<Guid, string>();

        return await context.ConsentDocuments
            .Where(d => ids.Contains(d.ConsentRecordId))
            .ToDictionaryAsync(d => d.ConsentRecordId, d => d.FileName, ct);
    }

    public Task AddAsync(ConsentDocument document, CancellationToken ct = default)
    {
        context.ConsentDocuments.Add(document);
        return Task.CompletedTask;
    }

    public void Remove(ConsentDocument document) => context.ConsentDocuments.Remove(document);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => context.SaveChangesAsync(ct);
}
