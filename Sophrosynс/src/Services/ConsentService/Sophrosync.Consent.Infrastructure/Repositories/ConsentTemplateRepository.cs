using Microsoft.EntityFrameworkCore;
using Sophrosync.Consent.Domain.Entities;
using Sophrosync.Consent.Domain.Enums;
using Sophrosync.Consent.Domain.Interfaces;
using Sophrosync.Consent.Infrastructure.Persistence;

namespace Sophrosync.Consent.Infrastructure.Repositories;

public sealed class ConsentTemplateRepository(ConsentDbContext context) : IConsentTemplateRepository
{
    public async Task<ConsentTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.ConsentTemplates.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task AddAsync(ConsentTemplate entity, CancellationToken ct = default)
        => await context.ConsentTemplates.AddAsync(entity, ct);

    public void Update(ConsentTemplate entity) => context.ConsentTemplates.Update(entity);

    public void Remove(ConsentTemplate entity) => context.ConsentTemplates.Remove(entity);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await context.SaveChangesAsync(ct);

    public async Task<IReadOnlyList<ConsentTemplate>> GetAllAsync(CancellationToken ct = default)
        => await context.ConsentTemplates.OrderByDescending(t => t.CreatedAt).ToListAsync(ct);

    public async Task<ConsentTemplate?> GetPublishedForPurposeAsync(ConsentPurpose purpose, CancellationToken ct = default)
        => await context.ConsentTemplates
            .FirstOrDefaultAsync(t => t.Purpose == purpose && t.Status == ConsentTemplateStatus.Published, ct);
}
