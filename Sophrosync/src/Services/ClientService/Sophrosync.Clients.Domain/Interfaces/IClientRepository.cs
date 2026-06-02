using Sophrosync.Clients.Domain.Entities;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Clients.Domain.Interfaces;

/// <summary>
/// Persistence contract for the Client aggregate.
/// </summary>
public interface IClientRepository : IRepository<Client>
{
    /// <summary>
    /// Returns all non-deleted clients visible to the current tenant.
    /// </summary>
    Task<IReadOnlyList<Client>> GetAllAsync(CancellationToken ct = default);
}
