using Sophrosync.Consent.Domain.Entities;
using Sophrosync.Consent.Domain.Enums;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Consent.Domain.Interfaces;

/// <summary>
/// Append-only repository — AddAsync and SaveChangesAsync only.
/// GetByIdAsync is for reads. Update and Remove are intentionally not used.
/// </summary>
public interface IConsentRecordRepository : IRepository<ConsentRecord>
{
    Task<IReadOnlyList<ConsentRecord>> GetHistoryForClientAsync(Guid clientId, CancellationToken ct = default);
    Task<ConsentRecord?> GetLatestForClientAndPurposeAsync(Guid clientId, ConsentPurpose purpose, CancellationToken ct = default);
    Task<IReadOnlyList<ConsentRecord>> GetAllForAuditAsync(CancellationToken ct = default);
}
