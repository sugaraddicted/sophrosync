using Sophrosync.Consent.Domain.Entities;
using Sophrosync.Consent.Domain.Enums;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Consent.Domain.Interfaces;

public interface IConsentRequestRepository : IRepository<ConsentRequest>
{
    Task<IReadOnlyList<ConsentRequest>> GetPendingForClientAsync(Guid clientId, CancellationToken ct = default);
    Task<IReadOnlyList<ConsentRequest>> GetOverdueAsync(DateTime asOf, CancellationToken ct = default);
    Task<ConsentRequest?> GetActiveForClientAndPurposeAsync(Guid clientId, ConsentPurpose purpose, CancellationToken ct = default);
}
