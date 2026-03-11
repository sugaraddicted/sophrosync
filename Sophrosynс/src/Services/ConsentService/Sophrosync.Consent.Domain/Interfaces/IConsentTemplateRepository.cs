using Sophrosync.Consent.Domain.Entities;
using Sophrosync.Consent.Domain.Enums;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Consent.Domain.Interfaces;

public interface IConsentTemplateRepository : IRepository<ConsentTemplate>
{
    Task<IReadOnlyList<ConsentTemplate>> GetAllAsync(CancellationToken ct = default);
    Task<ConsentTemplate?> GetPublishedForPurposeAsync(ConsentPurpose purpose, CancellationToken ct = default);
}
