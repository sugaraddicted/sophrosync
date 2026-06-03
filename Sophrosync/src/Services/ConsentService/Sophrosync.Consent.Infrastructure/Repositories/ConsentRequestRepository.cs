using Microsoft.EntityFrameworkCore;
using Sophrosync.Consent.Domain.Entities;
using Sophrosync.Consent.Domain.Enums;
using Sophrosync.Consent.Domain.Interfaces;
using Sophrosync.Consent.Infrastructure.Persistence;

namespace Sophrosync.Consent.Infrastructure.Repositories;

public sealed class ConsentRequestRepository(ConsentDbContext context) : IConsentRequestRepository
{
    public async Task<ConsentRequest?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.ConsentRequests.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task AddAsync(ConsentRequest entity, CancellationToken ct = default)
        => await context.ConsentRequests.AddAsync(entity, ct);

    public void Update(ConsentRequest entity) => context.ConsentRequests.Update(entity);

    public void Remove(ConsentRequest entity) => context.ConsentRequests.Remove(entity);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await context.SaveChangesAsync(ct);

    public async Task<IReadOnlyList<ConsentRequest>> GetPendingForClientAsync(Guid clientId, CancellationToken ct = default)
        => await context.ConsentRequests
            .Where(r => r.ClientId == clientId && r.Status == ConsentRequestStatus.Pending)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ConsentRequest>> GetOverdueAsync(DateTime asOf, CancellationToken ct = default)
        => await context.ConsentRequests
            .Where(r => r.Status == ConsentRequestStatus.Pending && r.ExpiresAt <= asOf)
            .ToListAsync(ct);

    public async Task<ConsentRequest?> GetActiveForClientAndPurposeAsync(Guid clientId, ConsentPurpose purpose, CancellationToken ct = default)
        => await context.ConsentRequests
            .Where(r => r.ClientId == clientId && r.Status == ConsentRequestStatus.Pending)
            .Join(context.ConsentTemplates.Where(t => t.Purpose == purpose),
                req => req.ConsentTemplateId, tmpl => tmpl.Id,
                (req, tmpl) => req)
            .FirstOrDefaultAsync(ct);
}
