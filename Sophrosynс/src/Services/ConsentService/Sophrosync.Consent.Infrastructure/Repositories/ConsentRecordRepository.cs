using Microsoft.EntityFrameworkCore;
using Sophrosync.Consent.Domain.Entities;
using Sophrosync.Consent.Domain.Enums;
using Sophrosync.Consent.Domain.Interfaces;
using Sophrosync.Consent.Infrastructure.Persistence;

namespace Sophrosync.Consent.Infrastructure.Repositories;

public sealed class ConsentRecordRepository(ConsentDbContext context) : IConsentRecordRepository
{
    public async Task<ConsentRecord?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.ConsentRecords.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task AddAsync(ConsentRecord entity, CancellationToken ct = default)
        => await context.ConsentRecords.AddAsync(entity, ct);

    // Append-only: these should never be called in normal operation
    public void Update(ConsentRecord entity) => throw new InvalidOperationException("ConsentRecord is append-only. Updates are not permitted.");
    public void Remove(ConsentRecord entity) => throw new InvalidOperationException("ConsentRecord is append-only. Deletions are not permitted.");

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await context.SaveChangesAsync(ct);

    public async Task<IReadOnlyList<ConsentRecord>> GetHistoryForClientAsync(Guid clientId, CancellationToken ct = default)
        => await context.ConsentRecords
            .Where(r => r.ClientId == clientId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

    public async Task<ConsentRecord?> GetLatestForClientAndPurposeAsync(Guid clientId, ConsentPurpose purpose, CancellationToken ct = default)
        => await context.ConsentRecords
            .Where(r => r.ClientId == clientId && r.Purpose == purpose)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<ConsentRecord>> GetAllForAuditAsync(CancellationToken ct = default)
        => await context.ConsentRecords.OrderByDescending(r => r.CreatedAt).ToListAsync(ct);
}
