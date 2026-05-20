using Sophrosync.Notes.Domain.Entities;

namespace Sophrosync.Notes.Domain.Interfaces;

/// <summary>
/// Persistence contract for the Note aggregate.
/// </summary>
public interface INoteRepository
{
    /// <summary>
    /// Returns a single note by its Id (respects the global tenant + soft-delete filter).
    /// </summary>
    Task<Note?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Returns all non-deleted notes visible to the current tenant.
    /// </summary>
    Task<List<Note>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns all non-deleted notes for the specified client within the current tenant.
    /// </summary>
    Task<List<Note>> GetByClientIdAsync(Guid clientId, CancellationToken ct = default);

    /// <summary>
    /// Adds a new Note to the change tracker and returns it.
    /// Caller must invoke <see cref="SaveChangesAsync"/> to persist.
    /// </summary>
    Task<Note> AddAsync(Note note, CancellationToken ct = default);

    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    Task SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns the count of non-deleted notes with the specified status for the current tenant.
    /// </summary>
    Task<int> CountByStatusAsync(string status, CancellationToken ct = default);

    /// <summary>
    /// Returns the count of non-deleted notes with the specified status across ALL tenants.
    /// Bypasses the global tenant query filter — for internal/reporting use only.
    /// </summary>
    Task<int> CountByStatusGlobalAsync(string status, CancellationToken ct = default);

    /// <summary>
    /// Returns note counts grouped by status across ALL tenants in a single query.
    /// Bypasses the global tenant query filter — for internal/reporting use only.
    /// </summary>
    Task<Dictionary<string, int>> GetStatusCountsGlobalAsync(CancellationToken ct = default);
}
